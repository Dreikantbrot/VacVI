
namespace Evo_VI.classes.dialog
{
    public class DialogLine
    {
        #region Enums
        /// <summary> The importance of a line of dialog.
        /// </summary>
        public enum DialogImportance { LOW, NORMAL, HIGH, CRITICAL };
        #endregion


        #region Variables
        string _text;
        DialogImportance _importance;
        #endregion


        #region Properties
        /// <summary> Returns or sets the line's importance.
        /// </summary>
        public DialogImportance Importance
        {
            get { return _importance; }
            set { _importance = value; }
        }


        /// <summary> Returns or sets the line's text.
        /// </summary>
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }
        #endregion


        /// <summary> Creates a new instance for a line of dialog.
        /// </summary>
        /// <param name="pText">The line's text</param>
        /// <param name="pImportance">The importance this line has over others.</param>
        public DialogLine(string pText, DialogImportance pImportance = DialogImportance.NORMAL)
        {
            this._text = pText;
            this._importance = pImportance;
        }
    }
}
