// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebView;

namespace Eto.Blazor
{
	/// <summary>
	/// Describes a root component that can be added to a <see cref="BlazorWebView"/>.
	/// </summary>
	public class RootComponent
	{
		/// <summary>
		/// Constructs an instance of <see cref="RootComponent"/>.
		/// </summary>
		/// <param name="selector">The CSS selector string that specifies where in the document the component should be placed. This must be unique among the root components within the <see cref="BlazorWebView"/>.</param>
		/// <param name="componentType">The type of the root component. This type must implement <see cref="IComponent"/>.</param>
		/// <param name="parameters">An optional dictionary of parameters to pass to the root component.</param>
		public RootComponent(string selector, Type componentType, IDictionary<string, object?>? parameters)
		{
			if (string.IsNullOrWhiteSpace(selector))
			{
				throw new ArgumentException($"'{nameof(selector)}' cannot be null or whitespace.", nameof(selector));
			}

			Selector = selector;
			ComponentType = componentType ?? throw new ArgumentNullException(nameof(componentType));
			Parameters = parameters;
		}
		
		
		/// <summary>
		/// Gets or sets the CSS selector string that specifies where in the document the component should be placed.
		/// This must be unique among the root components within the <see cref="BlazorWebView"/>.
		/// </summary>
		public string Selector { get; }

		/// <summary>
		/// Gets or sets the type of the root component. This type must implement <see cref="IComponent"/>.
		/// </summary>
		public Type ComponentType { get; }

		/// <summary>
		/// Gets or sets an optional dictionary of parameters to pass to the root component.
		/// </summary>
		public IDictionary<string, object?>? Parameters { get; set; }

		public Task AddToWebViewManagerAsync(WebViewManager webViewManager)
		{
			// As a characteristic of XAML,we can't rely on non-default constructors. So we have to
			// validate that the required properties were set. We could skip validating this and allow
			// the lower-level renderer code to throw, but that would be harder for developers to understand.

			if (string.IsNullOrWhiteSpace(Selector))
			{
				throw new InvalidOperationException($"{nameof(RootComponent)} requires a value for its {nameof(Selector)} property, but no value was set.");
			}

			if (ComponentType is null)
			{
				throw new InvalidOperationException($"{nameof(RootComponent)} requires a value for its {nameof(ComponentType)} property, but no value was set.");
			}

			var parameterView = Parameters == null ? ParameterView.Empty : ParameterView.FromDictionary(Parameters);
			return webViewManager.AddRootComponentAsync(ComponentType, Selector, parameterView);
		}

		public Task RemoveFromWebViewManagerAsync(WebViewManager webviewManager)
		{
			if (string.IsNullOrWhiteSpace(Selector))
			{
				throw new InvalidOperationException($"{nameof(RootComponent)} requires a value for its {nameof(Selector)} property, but no value was set.");
			}
			return webviewManager.RemoveRootComponentAsync(Selector);
		}
	}
}