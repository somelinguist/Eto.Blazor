using System;
using System.Collections.Specialized;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.FileProviders;
using Eto.Forms;

namespace Eto.Blazor
{
	/// <summary>
	/// A <see cref="Control"/> that can render Blazor content.
	/// </summary>
	[Handler(typeof(BlazorWebView.IHandler))]
	public class BlazorWebView : Control
	{
		public const string AppHostAddress = "0.0.0.0";
		
		new IHandler Handler { get { return (IHandler)base.Handler; } }
		
		/// <summary>
		/// Gets or sets the path to the HTML file to render.
		/// <para>This is an app relative path to the file such as <c>wwwroot\index.html</c></para>
		/// </summary>
		public string? HostPage
		{
			get => Handler.HostPage;
			set
			{
				Handler.HostPage = value;
				//OnHostPagePropertyChanged();
			}
		}
		
		public RootComponentsCollection RootComponents => Handler.RootComponents;
		
		/// <summary>
		/// Gets or sets an <see cref="IServiceProvider"/> containing services to be used by this control and also by application code.
		/// This property must be set to a valid value for the Razor components to start.
		/// </summary>
		public IServiceProvider? Services
		{
			get => Handler.Services;
			set
			{
				Handler.Services = value;
				//OnServicesPropertyChanged();
			}
		}

		public const string UrlLoadingEvent = "BlazorWebView.UrlLoading";
		
		// /// <summary>
		// /// Allows customizing how links are opened.
		// /// By default, opens internal links in the webview and external links in an external app.
		// /// </summary>
		public event EventHandler<UrlLoadingEventArgs> UrlLoading
		{
			add { Properties.AddHandlerEvent(UrlLoadingEvent, value); }
			remove { Properties.AddHandlerEvent(UrlLoadingEvent, value); }
		}
		
		/// <summary>
		/// Raises the <see cref="UrlLoading"/> event.
		/// </summary>
		/// <param name="args">Event arguments.</param>
		void OnUrlLoading(UrlLoadingEventArgs args)
		{
			Properties.TriggerEvent(UrlLoadingEvent, this, args);
		}

		public const string BlazorWebViewInitializingEvent = "BlazorWebView.BlazorWebViewInitializing";
		
		/// <summary>
		/// Raised before the web view is initialized. On some platforms this enables customizing the web view configuration.
		/// </summary>
		public event EventHandler<BlazorWebViewInitializingEventArgs> BlazorWebViewInitializing
		{
			add { Properties.AddHandlerEvent(BlazorWebViewInitializingEvent, value); }
			remove { Properties.AddHandlerEvent(BlazorWebViewInitializingEvent, value); }
		}
		
		/// <summary>
		/// Raises the <see cref="BlazorWebViewInitializing"/> event.
		/// </summary>
		/// <param name="args">Event arguments.</param>
		void OnBlazorWebViewInitializing(BlazorWebViewInitializingEventArgs args)
		{
			Properties.TriggerEvent(BlazorWebViewInitializingEvent, this, args);
		}
		
		public const string BlazorWebViewInitializedEvent = "BlazorWebView.BlazorWebViewInitialized";
		
		/// <summary>
		/// Raised after the web view is initialized but before any component has been rendered. The event arguments provide the instance of the platform-specific web view control.
		/// </summary>
		public event EventHandler<BlazorWebViewInitializedEventArgs> BlazorWebViewInitialized
		{
			add { Properties.AddHandlerEvent(BlazorWebViewInitializedEvent, value); }
			remove { Properties.AddHandlerEvent(BlazorWebViewInitializedEvent, value); }
		}
		
		/// <summary>
		/// Raises the <see cref="BlazorWebViewInitializing"/> event.
		/// </summary>
		/// <param name="args">Event arguments.</param>
		void OnBlazorWebViewInitialized(BlazorWebViewInitializingEventArgs args)
		{
			Properties.TriggerEvent(BlazorWebViewInitializedEvent, this, args);
		}

		public IFileProvider CreateFileProvider(string contentRootDir) =>
			Handler.CreateFileProvider(contentRootDir);

		public new interface IHandler : Control.IHandler
		{

			public string? HostPage { get; set; }
			
			public IServiceProvider? Services { get; set; }

			//JSComponentConfigurationStore JSComponents { get; }

			public RootComponentsCollection RootComponents { get; }
			
			public IFileProvider CreateFileProvider(string contentRootDir);
  
		}
		
		static readonly object callback = new Callback();
		
		/// <summary>
		/// Gets an instance of an object used to perform callbacks to the widget from handler implementations
		/// </summary>
		/// <returns>The callback instance to use for this widget</returns>
		protected override object GetCallback()
		{
			return callback;
		}

		public new interface ICallback : Control.ICallback
		{
			/// <summary>
			/// Raises the <see cref="UrlLoading"/> event.
			/// </summary>
			void OnUrlLoading(BlazorWebView widget, UrlLoadingEventArgs e);

			/// <summary>
			/// Raises the <see cref="BlazorWebViewInitializing"/> event.
			/// </summary>
			void OnBlazorWebViewInitializing(BlazorWebView widget, BlazorWebViewInitializingEventArgs e);

			/// <summary>
			/// Raises the <see cref="BlazorWebViewInitializing"/> event.
			/// </summary>
			void OnBlazorWebViewInitialized(BlazorWebView widget, BlazorWebViewInitializingEventArgs e);
		}
		
		protected new class Callback : Control.Callback, ICallback
		{
			/// <summary>
			/// Raises the <see cref="UrlLoading"/> event.
			/// </summary>
			public void OnUrlLoading(BlazorWebView widget, UrlLoadingEventArgs e)
			{
				using (widget.Platform.Context)
					widget.OnUrlLoading(e);
			}

			/// <summary>
			/// Raises the <see cref="BlazorWebViewInitializing"/> event.
			/// </summary>
			public void OnBlazorWebViewInitializing(BlazorWebView widget, BlazorWebViewInitializingEventArgs e)
			{
				using (widget.Platform.Context)
					widget.OnBlazorWebViewInitializing(e);
			}

			/// <summary>
			/// Raises the <see cref="BlazorWebViewInitializing"/> event.
			/// </summary>
			public void OnBlazorWebViewInitialized(BlazorWebView widget, BlazorWebViewInitializingEventArgs e)
			{
				using (widget.Platform.Context)
					widget.OnBlazorWebViewInitialized(e);
			}
		}
	}
}