using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Rigsarkiv.AthenaWPF
{
    /// <summary>
    /// 
    /// </summary>
    public class StructureViewModel : INotifyDataErrorInfo
    {
        private readonly Dictionary<string, string> _validationErrors = new Dictionary<string, string>();

        private string _text;
        /// <summary>
        /// 
        /// </summary>
        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Validate()
        {
            bool isValid = !string.IsNullOrEmpty(_text);
            bool contains = _validationErrors.ContainsKey(nameof(Text));
            if (!isValid && !contains)
                _validationErrors.Add(nameof(Text), "Mandatory field!");
            else if (isValid && contains)
                _validationErrors.Remove(nameof(Text));

            if (ErrorsChanged != null)
                ErrorsChanged(this, new DataErrorsChangedEventArgs(nameof(Text)));
        }
        /// <summary>
        /// 
        /// </summary>
        public bool HasErrors => _validationErrors.Count > 0;
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public IEnumerable GetErrors(string propertyName)
        {
            string message;
            if (_validationErrors.TryGetValue(propertyName, out message))
            {
                return new List<string> { message };
            }
            return null;
        }
    }
}
