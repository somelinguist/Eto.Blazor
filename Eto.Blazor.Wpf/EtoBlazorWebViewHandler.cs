using System;
using sw = System.Windows;
using swc = System.Windows.Controls;
using Eto.Forms;


using Eto.Wpf.Forms;

using eb = Eto.Blazor;
using Microsoft.Extensions.FileProviders;
using asp = Microsoft.AspNetCore.Components.WebView;
using aspw = Microsoft.AspNetCore.Components.WebView.Wpf;

namespace Eto.Blazor.Wpf
{
    public class EtoBlazorWebViewHandler : WpfControl<aspw.BlazorWebView, eb.BlazorWebView, eb.BlazorWebView.ICallback>, eb.BlazorWebView.IHandler
    {
        private eb.EtoDispatcher _dispatcher;
        private eb.RootComponentsCollection _rootComponents;
        public EtoBlazorWebViewHandler()
        {
            Control = new aspw.BlazorWebView();
            //Control.BlazorWebViewInitializing += (s, e) => Callback.OnBlazorWebViewInitializing(Widget, new BlazorWebViewInitializingEventArgs()
            //{
            //    // Configuration = Config
            //    Stuff = "Config"
            //});
            //Control.BlazorWebViewInitialized += (s, e) => Callback.OnBlazorWebViewInitialized(Widget, new eb.BlazorWebViewInitializedEventArgs()
            //{ WebView=Control.WebView})
            Control.UrlLoading += (s, e) => Callback.OnUrlLoading(Widget, new eb.UrlLoadingEventArgs(e.Url,  e.UrlLoadingStrategy == asp.UrlLoadingStrategy.OpenInWebView ? eb.UrlLoadingStrategy.OpenInWebView : eb.UrlLoadingStrategy.OpenExternally));
            _rootComponents = new eb.RootComponentsCollection();
            _rootComponents.CollectionChanged += OnRootComponentsCollectionChanged;
            _dispatcher = new eb.EtoDispatcher(Application.Instance);
        }

        public string? HostPage { get => Control.HostPage; set => Control.HostPage = value; }
        public IServiceProvider? Services { get => Control.Services; set => Control.Services = value; }

        public RootComponentsCollection RootComponents
        { get => _rootComponents!; }

        private void OnRootComponentsCollectionChanged(object? sender, global::System.Collections.Specialized.NotifyCollectionChangedEventArgs eventArgs)
        {
            
            // Dispatch because this is going to be async, and we want to catch any errors
            //_ = _dispatcher.InvokeAsync(async () =>
            //{
                var newItems = (eventArgs.NewItems ?? Array.Empty<RootComponent>()).Cast<RootComponent>();
                var oldItems = (eventArgs.OldItems ?? Array.Empty<RootComponent>()).Cast<RootComponent>();

                foreach (var item in newItems.Except(oldItems))
                {
                    var component = new aspw.RootComponent()
                    {
                        ComponentType = item.ComponentType,
                        Selector = item.Selector,
                        Parameters = item.Parameters
                    };
                    Control.RootComponents.Add(component);
                }

                foreach (var item in oldItems.Except(newItems))
                {
                    foreach (var component in Control.RootComponents.Where( c => c.Selector == item.Selector))
                    { 
                        Control.RootComponents.Remove(component);
                        break;
                    }
                }
            //});
        }

        public IFileProvider CreateFileProvider(string contentRootDir)
        {
            return Control.CreateFileProvider(contentRootDir);
        }
    }
}

