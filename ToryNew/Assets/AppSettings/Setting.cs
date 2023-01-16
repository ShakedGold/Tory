using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tory.Views;
using Windows.Storage;

namespace ToryNew.Assets.AppSettings {
    public class Setting<T> where T : IConvertible, IComparable {
        private string _propertyName;

        public string Name { 
            get {
                return _propertyName;
            }
            set {
                _propertyName = value;
            }
        }
        public T Value {
            get {
                return getObject();
            }
            set {
                if (isEnum())
                    ApplicationData.Current.LocalSettings.Values[Name] = (int)Enum.Parse(typeof(T), value.ToString());
                else
                    ApplicationData.Current.LocalSettings.Values[Name] = value;
            }
        }

        public Setting(T def, string name) {
            Name = name;
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(Name)) Value = def;
            else Value = getObject();
        }
        public Setting(T def) : this(def, typeof(T).Name) { }

        public override string ToString() {
            return Value.ToString();
        }

        public bool isEnum() {
            return typeof(T).IsEnum;
        }

        public bool Update(T val) {
            if (val == null) return false;
            Value = val;
            return true;
        }

        public T getObject() {
            return (T)ApplicationData.Current.LocalSettings.Values[Name];
        }
    }
}
