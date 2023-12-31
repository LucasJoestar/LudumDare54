// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Special drawer for fields with the attribute <see cref="MinAttribute"/> (inherit from <see cref="EnhancedPropertyDrawer"/>).
    /// </summary>
    [CustomDrawer(typeof(MinAttribute))]
    public class MinPropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        public override void OnValueChanged(SerializedProperty _property)
        {
            MinAttribute _attribute = Attribute as MinAttribute;
            float _minValue;

            // Get minimum allowed value and floor property value.
            if (_attribute.MinMember == null)
            {
                _minValue = _attribute.MinValue;
            }
            else if (!_attribute.MinMember.Value.GetValue(_property, out _minValue))
                return;

            EnhancedEditorUtility.FloorSerializedPropertyValue(_property, _minValue);
        }
        #endregion
    }
}
