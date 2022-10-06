// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.FileProviders;
using Eto.Forms;
using wk = WebKit;

namespace Eto.Blazor.Mac
{
    public class EtoMacWebViewManager : WebViewManager
    {
        private readonly EtoBlazorWebViewHandler _etoBlazorWebViewHandler;
        private readonly wk.WKWebView _webview;
        private readonly string _contentRootRelativeToAppRoot;

        public EtoMacWebViewManager(EtoBlazorWebViewHandler handler, wk.WKWebView webview, IServiceProvider provider, Dispatcher dispatcher, Uri appBaseUri, IFileProvider fileProvider, string contentRootRelativeToAppRoot, string hostPageRelativePath)
            : base(provider, dispatcher, appBaseUri, fileProvider, handler.RootComponents!.JSComponents, hostPageRelativePath)
        {
	        _etoBlazorWebViewHandler = handler;
	        _webview = webview ?? throw new ArgumentNullException(nameof(webview));
	        _contentRootRelativeToAppRoot = contentRootRelativeToAppRoot;

			InitializeWebView();
        }

        internal void MessageReceivedInternal(Uri uri, string message)
        {
            MessageReceived(uri, message);
        }

        protected override void NavigateCore(Uri absoluteUri)
        {
            using var nsUrl = new NSUrl(absoluteUri.ToString());
            using var request = new NSUrlRequest(nsUrl);
            _webview.LoadRequest(request);
        }
        
        internal bool TryGetResponseContentInternal(string uri, bool allowFallbackOnHostPage, out int statusCode, out string statusMessage, out Stream content, out IDictionary<string, string> headers)
        {
	        var defaultResult = TryGetResponseContent(uri, allowFallbackOnHostPage, out statusCode, out statusMessage, out content, out headers);
	        var hotReloadedResult = StaticContentHotReloadManager.TryReplaceResponseContent(_contentRootRelativeToAppRoot, uri, ref statusCode, ref content, headers);
	        return defaultResult || hotReloadedResult;
        }

        protected override void SendMessage(string message)
        {
	        var messageJSStringLiteral = JavaScriptEncoder.Default.Encode(message);
	        _webview.EvaluateJavaScript(
		        javascript: $"__dispatchMessageCallback(\"{messageJSStringLiteral}\")",
		        completionHandler: (NSObject result, NSError error) => { });
        }
        
        private void InitializeWebView()
		{
			_webview.NavigationDelegate = new WebViewNavigationDelegate(_etoBlazorWebViewHandler);
			_webview.UIDelegate = new WebViewUIDelegate(_etoBlazorWebViewHandler);
		}

		internal sealed class WebViewUIDelegate : wk.WKUIDelegate
		{
			private static readonly string LocalOK = NSBundle.FromIdentifier("com.apple.UIKit").GetLocalizedString("OK");
			private static readonly string LocalCancel = NSBundle.FromIdentifier("com.apple.UIKit").GetLocalizedString("Cancel");
			private readonly EtoBlazorWebViewHandler _webView;

			public WebViewUIDelegate(EtoBlazorWebViewHandler webView)
			{
				_webView = webView ?? throw new ArgumentNullException(nameof(webView));
			}


			public override void RunJavaScriptAlertPanel(wk.WKWebView webView, string message, wk.WKFrameInfo frame, Action completionHandler)
			{
				MessageBox.Show(_webView.Widget, message);
				completionHandler();
			}

			public override void RunJavaScriptConfirmPanel(wk.WKWebView webView, string message, wk.WKFrameInfo frame, Action<bool> completionHandler)
			{
				var result = MessageBox.Show(_webView.Widget, message, MessageBoxButtons.YesNo) == DialogResult.Yes;
				completionHandler(result);
			}

			//public override void RunJavaScriptTextInputPanel(
			//	wk.WKWebView webView, string prompt, string? defaultText, wk.WKFrameInfo frame, Action<string> completionHandler)
			//{
				// TODO: Do we need to override RunJavaScriptTextInputPanel?
				// var result = MessageBox.Show(_webView.Widget, message, MessageBoxButtons.YesNo);
				// completionHandler(result.);
				
				// PresentAlertController(
				// 	webView,
				// 	prompt,
				// 	defaultText: defaultText,
				// 	okAction: x => completionHandler(x.TextFields[0].Text!),
				// 	cancelAction: _ => completionHandler(null!)
				// );
			//}

			private static string GetJsAlertTitle(wk.WKWebView webView)
			{
				// Emulate the behavior of UIWebView dialogs.
				// The scheme and host are used unless local html content is what the webview is displaying,
				// in which case the bundle file name is used.
				if (webView.Url != null && webView.Url.AbsoluteString != $"file://{NSBundle.MainBundle.BundlePath}/")
					return $"{webView.Url.Scheme}://{webView.Url.Host}";

				return new NSString(NSBundle.MainBundle.BundlePath).LastPathComponent;
			}
		}

		internal class WebViewNavigationDelegate : wk.WKNavigationDelegate
		{
			private readonly EtoBlazorWebViewHandler _webView;

			private wk.WKNavigation? _currentNavigation;
			private Uri? _currentUri;

			public WebViewNavigationDelegate(EtoBlazorWebViewHandler webView)
			{
				_webView = webView ?? throw new ArgumentNullException(nameof(webView));
			}

			public override void DidStartProvisionalNavigation(wk.WKWebView webView, wk.WKNavigation navigation)
			{
				_currentNavigation = navigation;
			}

			public override void DecidePolicy(wk.WKWebView webView, wk.WKNavigationAction navigationAction, Action<wk.WKNavigationActionPolicy> decisionHandler)
			{
				var requestUrl = navigationAction.Request.Url;
				UrlLoadingStrategy strategy;

				// TargetFrame is null for navigation to a new window (`_blank`)
				if (navigationAction.TargetFrame is null)
				{
					// Open in a new browser window regardless of UrlLoadingStrategy
					strategy = UrlLoadingStrategy.OpenExternally;
				}
				else
				{
					// Invoke the UrlLoading event to allow overriding the default link handling behavior
					var uri = new Uri(requestUrl.ToString());
					var callbackArgs = UrlLoadingEventArgs.CreateWithDefaultLoadingStrategy(uri, EtoBlazorWebViewHandler.AppOriginUri);
					// TODO: Fix OnUrlLoading
					_webView.Callback.OnUrlLoading(_webView.Widget, callbackArgs);
					strategy = callbackArgs.UrlLoadingStrategy;
				}

				if (strategy == UrlLoadingStrategy.OpenExternally)
				{
					NSWorkspace.SharedWorkspace.OpenUrl(requestUrl);
				}

				if (strategy != UrlLoadingStrategy.OpenInWebView)
				{
					// Cancel any further navigation as we've either opened the link in the external browser
					// or canceled the underlying navigation action.
					decisionHandler(wk.WKNavigationActionPolicy.Cancel);
					return;
				}

				if (navigationAction.TargetFrame!.MainFrame)
				{
					_currentUri = requestUrl;
				}

				decisionHandler(wk.WKNavigationActionPolicy.Allow);
			}

			public override void DidReceiveServerRedirectForProvisionalNavigation(wk.WKWebView webView, wk.WKNavigation navigation)
			{
				// We need to intercept the redirects to the app scheme because Safari will block them.
				// We will handle these redirects through the Navigation Manager.
				if (_currentUri?.Host == BlazorWebView.AppHostAddress)
				{
					var uri = _currentUri;
					_currentUri = null;
					_currentNavigation = null;
					if (uri is not null)
					{
						var request = new NSUrlRequest(new NSUrl(uri.AbsoluteUri));
						webView.LoadRequest(request);
					}
				}
			}

			public override void DidFailNavigation(wk.WKWebView webView, wk.WKNavigation navigation, NSError error)
			{
				_currentUri = null;
				_currentNavigation = null;
			}

			public override void DidFailProvisionalNavigation(wk.WKWebView webView, wk.WKNavigation navigation, NSError error)
			{
				_currentUri = null;
				_currentNavigation = null;
			}

			public override void DidCommitNavigation(wk.WKWebView webView, wk.WKNavigation navigation)
			{
				if (_currentUri != null && _currentNavigation == navigation)
				{
					// TODO: Determine whether this is needed
					//_webView.HandleNavigationStarting(_currentUri);
				}
			}

			public override void DidFinishNavigation(wk.WKWebView webView, wk.WKNavigation navigation)
			{
				if (_currentUri != null && _currentNavigation == navigation)
				{
					// TODO: Determine whether this is needed
					//_webView.HandleNavigationFinished(_currentUri);
					_currentUri = null;
					_currentNavigation = null;
				}
			}
		}
    }
}