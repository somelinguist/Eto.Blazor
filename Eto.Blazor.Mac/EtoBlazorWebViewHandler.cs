using System;
using System.Globalization;
using System.IO;
using System.Runtime.Versioning;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Eto.Forms;
using Eto.Mac.Forms;
using Eto.Blazor;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView;
using wk = WebKit;
using RectangleF = CoreGraphics.CGRect;

namespace Eto.Blazor.Mac
{
    
    /// <summary>
    /// The Mac handler for Eto <see cref="BlazorWebView"/>
    /// </summary>
    public class EtoBlazorWebViewHandler : MacView<wk.WKWebView, BlazorWebView, BlazorWebView.ICallback>, BlazorWebView.IHandler
    {
        private EtoMacWebViewManager? _webviewManager;
        
        internal const string AppOrigin = "app://" + BlazorWebView.AppHostAddress + "/";
        internal static readonly Uri AppOriginUri = new(AppOrigin);
        private const string BlazorInitScript = @"
			window.__receiveMessageCallbacks = [];
			window.__dispatchMessageCallback = function(message) {
				window.__receiveMessageCallbacks.forEach(function(callback) { callback(message); });
			};
			window.external = {
				sendMessage: function(message) {
					window.webkit.messageHandlers.webwindowinterop.postMessage(message);
				},
				receiveMessage: function(callback) {
					window.__receiveMessageCallbacks.push(callback);
				}
			};
			Blazor.start();
			(function () {
				window.onpageshow = function(event) {
					if (event.persisted) {
						window.location.reload();
					}
				};
			})();
		";
        
        public override NSView ContainerControl { get { return Control; } }
        
        public wk.WKWebViewConfiguration Config { get; set; } = new wk.WKWebViewConfiguration();
        
		internal BlazorWebViewDeveloperTools? DeveloperTools { get; set; }// => Services!.GetRequiredService<BlazorWebViewDeveloperTools>();

        [SupportedOSPlatform("macos12.0")]
        protected override wk.WKWebView CreateControl()
        {
			


            Callback.OnBlazorWebViewInitializing(Widget, new BlazorWebViewInitializingEventArgs()
            {
                // Configuration = Config
                Stuff = "Config"
            });
            
            Config.UserContentController.AddScriptMessageHandler(new WebViewScriptMessageHandler(MessageReceived), "webwindowinterop");
            Config.UserContentController.AddUserScript(new wk.WKUserScript(
	            new NSString(BlazorInitScript), wk.WKUserScriptInjectionTime.AtDocumentEnd, true));
            
            // iOS WKWebView doesn't allow handling 'http'/'https' schemes, so we use the fake 'app' scheme
            Config.SetUrlSchemeHandler(new SchemeHandler(this), urlScheme: "app");
            
            var webview = new wk.WKWebView(new CGRect(0, 0, 200, 200), Config)
            {
	            UnderPageBackgroundColor = NSColor.Clear,
	            AutoresizesSubviews = true
            };

            // VirtualView.BlazorWebViewInitialized(new BlazorWebViewInitializedEventArgs
            // {
	           //  WebView = webview
            // });

            _rootComponents = new RootComponentsCollection();
			_rootComponents.CollectionChanged += OnRootComponentsCollectionChanged;

            return webview;
        }

        public override void OnLoadComplete(EventArgs e)
        {
	        base.OnLoadComplete(e);
			// TODO: Is this the correct place to call this?
			// It has to be called after Services has been set, which unfortunately doesn't work in CreateControl()
			DeveloperTools = Services!.GetRequiredService<BlazorWebViewDeveloperTools>();
            Config.Preferences.SetValueForKey(NSObject.FromObject(DeveloperTools.Enabled), new NSString("developerExtrasEnabled"));
	        StartWebViewCoreIfPossible();
        }

        public string? HostPage { get; set; }
        
        public IServiceProvider? Services { get; set; }

        //JSComponentConfigurationStore JSComponents { get; }

        private RootComponentsCollection? _rootComponents;
        
        public RootComponentsCollection RootComponents
        {
	        get => _rootComponents!;
	        set
	        {
		        if (_rootComponents != null)
		        {
			        // Remove any previously-known root components and unhook events
			        _rootComponents.Clear();
			        _rootComponents.CollectionChanged -= OnRootComponentsCollectionChanged;
		        }

		        _rootComponents = value;

		        if (_rootComponents != null)
		        {
			        // Add new root components and hook events
			        if (_rootComponents.Count > 0 && _webviewManager != null)
			        {
				        _webviewManager.Dispatcher.AssertAccess();
				        foreach (var component in _rootComponents)
				        {
					        _ = component.AddToWebViewManagerAsync(_webviewManager);
				        }
			        }
			        _rootComponents.CollectionChanged += OnRootComponentsCollectionChanged;
		        }
	        }
        }

        private void OnRootComponentsCollectionChanged(object? sender, global::System.Collections.Specialized.NotifyCollectionChangedEventArgs eventArgs)
        {
	        // If we haven't initialized yet, this is a no-op
	        if (_webviewManager != null)
	        {
		        // Dispatch because this is going to be async, and we want to catch any errors
		        _ = _webviewManager.Dispatcher.InvokeAsync(async () =>
		        {
			        var newItems = eventArgs.NewItems!.Cast<RootComponent>();
			        var oldItems = eventArgs.OldItems!.Cast<RootComponent>();

			        foreach (var item in newItems.Except(oldItems))
			        {
				        await item.AddToWebViewManagerAsync(_webviewManager);
			        }

			        foreach (var item in oldItems.Except(newItems))
			        {
				        await item.RemoveFromWebViewManagerAsync(_webviewManager);
			        }
		        });
	        }
        }
        
        private void MessageReceived(Uri uri, string message)
        {
            _webviewManager?.MessageReceivedInternal(uri, message);
        }
        
        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
	        Control.StopLoading();

	        if (_webviewManager != null)
	        {
		        // Dispose this component's contents and block on completion so that user-written disposal logic and
		        // Blazor disposal logic will complete.
		        _webviewManager?
			        .DisposeAsync()
			        .AsTask()
			        .GetAwaiter()
			        .GetResult();

		        _webviewManager = null;
	        }
        }
        
        private bool RequiredStartupPropertiesSet =>
	        //_webview != null &&
	        HostPage != null &&
	        Services != null;

        private void StartWebViewCoreIfPossible()
        {
	        if (!RequiredStartupPropertiesSet ||
	            _webviewManager != null)
	        {
		        return;
	        }
	        if (Control == null)
	        {
		        throw new InvalidOperationException($"Can't start {nameof(BlazorWebView)} without native web view instance.");
	        }

	        // We assume the host page is always in the root of the content directory, because it's
	        // unclear there's any other use case. We can add more options later if so.
	        var contentRootDir = Path.GetDirectoryName(HostPage!) ?? string.Empty;
	        var hostPageRelativePath = Path.GetRelativePath(contentRootDir, HostPage!);

	        var fileProvider = new EtoMacAssetFileProvider(contentRootDir);

	        var files = fileProvider.GetDirectoryContents("");

	        _webviewManager = new EtoMacWebViewManager(
		        this,
		        Control,
		        Services!,
		        new EtoDispatcher(Application.Instance),
		        AppOriginUri,
		        fileProvider,
		        contentRootDir,
		        hostPageRelativePath);

	        //StaticContentHotReloadManager.AttachToWebViewManagerIfEnabled(_webviewManager);

	        if (RootComponents != null)
	        {
		        foreach (var rootComponent in RootComponents)
		        {
			        // Since the page isn't loaded yet, this will always complete synchronously
			        _ = rootComponent.AddToWebViewManagerAsync(_webviewManager);
		        }
	        }

	        _webviewManager?.Navigate("/");
        }

        public IFileProvider CreateFileProvider(string contentRootDir)
        {
	        if (Directory.Exists(contentRootDir))
	        {
		        // Typical case after publishing, or if you're copying content to the bin dir in development for some nonstandard reason
		        return new PhysicalFileProvider(contentRootDir);
	        }
	        else
	        {
		        // Typical case in development, as the files come from Microsoft.AspNetCore.Components.WebView.StaticContentProvider
		        // instead and aren't copied to the bin dir
		        return new NullFileProvider();
	        }
        }
        
        private sealed class WebViewScriptMessageHandler : NSObject, wk.IWKScriptMessageHandler
        {
            private Action<Uri, string> _messageReceivedAction;

            public WebViewScriptMessageHandler(Action<Uri, string> messageReceivedAction)
            {
                _messageReceivedAction = messageReceivedAction ?? throw new ArgumentNullException(nameof(messageReceivedAction));
            }

            public void DidReceiveScriptMessage(wk.WKUserContentController userContentController, wk.WKScriptMessage message)
            {
                if (message is null)
                {
                    throw new ArgumentNullException(nameof(message));
                }
                _messageReceivedAction(AppOriginUri, ((NSString)message.Body).ToString());
            }
        }
        
        private class SchemeHandler : NSObject, wk.IWKUrlSchemeHandler
		{
			private readonly EtoBlazorWebViewHandler _webViewHandler;

			public SchemeHandler(EtoBlazorWebViewHandler webViewHandler)
			{
				_webViewHandler = webViewHandler;
			}

			[Export("webView:startURLSchemeTask:")]
			[SupportedOSPlatform("macos12.0")]
			public void StartUrlSchemeTask(wk.WKWebView webView, wk.IWKUrlSchemeTask urlSchemeTask)
			{
				var responseBytes = GetResponseBytes(urlSchemeTask.Request.Url.AbsoluteString, out var contentType, statusCode: out var statusCode);
				if (statusCode == 200)
				{
					using (var dic = new NSMutableDictionary<NSString, NSString>())
					{
						dic.Add((NSString)"Content-Length", (NSString)(responseBytes.Length.ToString(CultureInfo.InvariantCulture)));
						dic.Add((NSString)"Content-Type", (NSString)contentType);
						// Disable local caching. This will prevent user scripts from executing correctly.
						dic.Add((NSString)"Cache-Control", (NSString)"no-cache, max-age=0, must-revalidate, no-store");
						using var response = new NSHttpUrlResponse(urlSchemeTask.Request.Url, statusCode, "HTTP/1.1", dic);
						urlSchemeTask.DidReceiveResponse(response);
					}
					urlSchemeTask.DidReceiveData(NSData.FromArray(responseBytes));
					urlSchemeTask.DidFinish();
				}
			}

			private byte[] GetResponseBytes(string? url, out string contentType, out int statusCode)
			{
				var allowFallbackOnHostPage = AppOriginUri.IsBaseOfPage(url);
				url = QueryStringHelper.RemovePossibleQueryString(url);
				if (_webViewHandler._webviewManager!.TryGetResponseContentInternal(url, allowFallbackOnHostPage, out statusCode, out var statusMessage, out var content, out var headers))
				{
					statusCode = 200;
					using var ms = new MemoryStream();

					content.CopyTo(ms);
					content.Dispose();

					contentType = headers["Content-Type"];

					return ms.ToArray();
				}
				else
				{
					statusCode = 404;
					contentType = string.Empty;
					return Array.Empty<byte>();
				}
			}

			[Export("webView:stopURLSchemeTask:")]
			public void StopUrlSchemeTask(wk.WKWebView webView, wk.IWKUrlSchemeTask urlSchemeTask)
			{
			}
		}
    }
}

