Eto.Blazor
==========
### A library to use [Eto.Forms](https://github.com/picoe/Eto) to develop cross-platform desktop [ASP.NET Core Blazor Hybrid apps](https://learn.microsoft.com/en-us/aspnet/core/blazor/hybrid/?view=aspnetcore-6.0)

Description
-----------
This is a first attempt to support the BlazorWebView component in Eto.Forms. It's very rough, and there are likely bugs.
So far, it is able to load the sample counter app, but hasn't been tested beyond that.

At best, this could only be a side project for me, so I'd be glad if any one would rather contribute or take over.

Supported platforms and notes
-------------------
1. **Eto.Wpf** - this wraps the official Wpf BlazorWebView control
2. **Eto.WinForms** this wraps the official Windows Forms BlazorWebViewControl
3. **Eto.Mac** _(net6.0-macos only)_ - this uses a custom BlazorWebView implementation based on the official Maui iOS/macCatalyst control.

**Eto.Gtk** is _not supported_ at the moment, because I really don't know what to do. Maybe follow these issues: [1](https://github.com/GtkSharp/GtkSharp/pull/274), [2](https://github.com/lytico/maui/issues/9), [3](https://github.com/jsuarezruiz/maui-linux/issues/28)

Samples/ How-to
---------------
There is an example Eto.Forms app in the samples folder that shows how to load a simple Counter.razor file.

Note that for each platform, you need to make sure to hook to the platform specific control (`BlazorWebView.IHandler`) and the platform specific hook (`IEtoBlazorWebViewAdder`) to enable BlazorWebView support.

Adding the BlazorWebView control to an Eto.Forms control should be similar to following the instructions for building a [Windows Forms Blazor app](https://learn.microsoft.com/en-us/aspnet/core/blazor/hybrid/tutorials/windows-forms?view=aspnetcore-6.0) in Microsoft's documentation.

Not implemented yet
-------------------
Official BlazorWebView components on Windows Forms, WPF, and Maui let you configure and accesss the internal webview during and after initialization of the control, but I haven't figured out the best way to do this using Eto.Forms' WebView control.

Probably some other things.

## License
Many parts of the Mac implementation are copied and adapted from the [dotnet/maui](https://github.com/dotnet/maui/tree/main/src/BlazorWebView/src) source, copyright (c) .NET Foundation and licensed under the Apache License, Version 2.0.


