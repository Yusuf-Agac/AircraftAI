using System.Globalization;
using System.Threading;
using UnityEngine;
using UnityEditor;

namespace WorldComposer
{
    [InitializeOnLoad]
    public static class FixCultureEditor
    {
        static FixCultureEditor()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        }
    }
}

