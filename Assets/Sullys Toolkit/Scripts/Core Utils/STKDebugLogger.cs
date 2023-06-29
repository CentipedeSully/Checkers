using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SullysToolkit
{
    public static class STKDebugLogger
    {
        public static void LogStatement(bool isDebugActive, string statement)
        {
            if (isDebugActive)
                Debug.Log(statement);
        }

        public static void LogWarning(string statement)
        {
            Debug.LogWarning($"STK Warning: {statement}");
        }

        public static void LogError(string statement)
        {
            Debug.LogError($"STK Error: {statement}");
        }
    }

}
