using Avalonia.Controls;
using Avalonia.Controls.Templates;
using FastTelecom.AvaloniaUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace FastTelecom.AvaloniaUI
{

    public sealed class ViewLocator : IDataTemplate
    {
        public Control Build(object? param)
        {
            if (param is null)
                return new TextBlock { Text = "No view model provided." };

            var vmType = param.GetType();
            var viewName = vmType.FullName!.Replace("ViewModels", "Views")
                                           .Replace("ViewModel", "View");

            var viewType = Type.GetType(viewName);

            if (viewType is not null)
                return (Control)Activator.CreateInstance(viewType)!;

            return new TextBlock { Text = $"View not found: {viewName}" };
        }

        public bool Match(object? data) => data is ViewModelBase;
    }

}
