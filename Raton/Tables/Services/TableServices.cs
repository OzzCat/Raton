using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using System.Collections.Generic;
using Raton.Tables.Models;
using System;
using System.Reflection.Metadata;
using OfficeOpenXml.FormulaParsing;

namespace Raton.Tables.Services
{
    public static class TableServices
    {
        public static void SetupStringPropertyAndCheckDirty<T>(T tModel, string propName, string? value, bool isColumnNullable = false) where T : ITableModel
        {
            if (isColumnNullable)
                if (string.IsNullOrEmpty(value))
                {
                    var box = MessageBoxManager
                            .GetMessageBoxStandard("Yapi", "This cell cannot be empty",
                            ButtonEnum.Ok);

                    box.ShowWindowAsync();

                    return;
                }

            var type = typeof(T);
            var property = type.GetProperty(propName);

            if (property == null)
                throw new NotImplementedException("No such property in your model, dummy");

            if (property.GetValue(tModel) is not null && !(property.GetValue(tModel) is string))
            {
                throw new InvalidOperationException(
                                                    $"Command requires parameters of type {typeof(string)}, but received parameter of type {property.GetType().FullName}.");
            }

            if (string.Compare(property.GetValue(tModel) as string, value) != 0)
            {
                property.SetValue(tModel, value);
                tModel.IsDirty = true;
            }
        }
    }
}
