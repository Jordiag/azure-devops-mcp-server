using System.Text;

namespace Dotnet.AzureDevOps.Core.Common;

public static class ExceptionExtensions
{
    public static string DumpFullException(this Exception ex, bool includeData = true)
    {
        var stringBuilder = new StringBuilder();
        DumpExceptionRecursive(ex, stringBuilder, includeData);
        return stringBuilder.ToString();
    }

    private static void DumpExceptionRecursive(Exception ex, StringBuilder stringBuilder, bool includeData, int level = 0)
    {
        if(ex == null)
            return;

        string indent = new(' ', level * 2);
        stringBuilder.AppendLine($"{indent}Exception Type: {ex.GetType().FullName}");
        stringBuilder.AppendLine($"{indent}Message       : {ex.Message}");
        stringBuilder.AppendLine($"{indent}Source        : {ex.Source}");
        stringBuilder.AppendLine($"{indent}TargetSite    : {ex.TargetSite}");
        stringBuilder.AppendLine($"{indent}StackTrace    :");
        stringBuilder.AppendLine($"{indent}{ex.StackTrace}");

        if(includeData && ex.Data?.Count > 0)
        {
            stringBuilder.AppendLine($"{indent}Data:");
            foreach(object? key in ex.Data.Keys)
            {
                stringBuilder.AppendLine($"{indent}  {key}: {ex.Data[key]}");
            }
        }

        if(ex.InnerException != null)
        {
            stringBuilder.AppendLine($"{indent}Inner Exception:");
            DumpExceptionRecursive(ex.InnerException, stringBuilder, includeData, level + 1);
        }
    }
}