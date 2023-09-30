// [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "Namespace", "Assembly", "Class")]

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

#pragma warning disable
internal class CodeSamples : EditorWindow {
    // --- Inter-Region Title --- \\

    // ===== Inside Region Title ===== \\

    // -------------------------------------------
    // Button
    // -------------------------------------------

    // ----- Local Methods ----- \\

    /// <summary>
    /// Unknown value.
    /// </summary>
    private const int Value = 1;

    // -----------------------

    /// <inheritdoc cref="Doc(bool)"/>
    public void Doc() {
        using (var _scope = new GUILayout.HorizontalScope()) {

        }
    }
    
    /// <summary>
    /// This is a documentation method.)
    /// </summary>
    /// <param name="_value">Random value.</param>
    /// <returns>
    /// <inheritdoc cref="Doc(bool)" path="/param[@name='_value']"/>
    /// </returns>
    public bool Doc(bool _value) => _value;

    /// <summary>
    /// This is another documentation method.
    /// </summary>
    /// <param name="_value"><inheritdoc cref="Value" path="/summary"/></param>
    /// <param name="_bool"><inheritdoc cref="Doc(bool)" path="/param[@name='_value']"/></param>
    /// <returns><inheritdoc cref="Doc(bool)" path="/returns"/></returns>
    public int Doc(int _value, bool _bool) => _bool ? _value : 0;

    // -------------------------------------------
    // Constructor(s)
    // -------------------------------------------

    /// <summary>
    /// Returns the first <see cref="CodeSamples"/> currently on screen.
    /// <br/> Creates and shows a new instance if there is none.
    /// </summary>
    /// <returns><see cref="CodeSamples"/> instance on screen.</returns>
    public static CodeSamples GetWindow() {
        CodeSamples _window = GetWindow<CodeSamples>("My Window");
        _window.Show();
        
        return _window;
    }

    /// <summary>
    /// Contains multiple <see cref="CodeSamples"/>-related extension methods.
    /// </summary>
    private class CodeSamplesExtensions { }
}
#endif
