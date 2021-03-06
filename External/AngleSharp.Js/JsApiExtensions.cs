namespace AngleSharp.Js
{
    using AngleSharp.Dom;
    using AngleSharp.Scripting;
    using System;

    /// <summary>
    /// Useful extensions for the DOM.
    /// </summary>
    public static class JsApiExtensions
    {
        /// <summary>
        /// Executes the given script code in the context of the document.
        /// </summary>
        /// <param name="document">The document as context.</param>
        /// <param name="scriptCode">The script to run.</param>
        /// <returns>The result of running the script, if any.</returns>
        public static Object ExecuteScript(this IDocument document, String scriptCode)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            
            var service = document?.Context.GetService<JsScriptingService>();
            return service?.EvaluateScript(document, scriptCode);
        }
    }
}
