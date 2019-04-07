using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Micro.WinForms {
    public class EnumItem<T> {
        public readonly T value;
        public EnumItem(T v)
            => value = v;
        public override string ToString()
            => value?.ToString();

        public static implicit operator T(EnumItem<T> i)
            => i.value;
    }

    public enum Enums {
        None = 0,
        Users = 1
    }
    public enum Users {
        a, b, c
    }

    public class EnumConfig<E> where E : Enum {
        const string IENUMERABLE = "System.Collections.Generic.IEnumerable";
        static Type eiType = typeof(EnumItem<>);
        static MethodInfo
            toArray = typeof(Enumerable).GetMethod("ToArray"),
            select = typeof(Enumerable).GetMethods().Where(m => m.Name == "Select").First(),
            cast = typeof(Enumerable).GetMethod("Cast");
        protected internal Dictionary<E, Array> allEnums;
        protected internal Dictionary<E, object[]> allItems;

        public EnumConfig(params Type[] enumTypes) {
            var values = Enum.GetValues(typeof(E));
            int len = values.Length;

            if (enumTypes.Length < len)
                throw new ArgumentException("The enumeration has more values than the amount of passed types.", nameof(enumTypes));
            foreach(var t in enumTypes) {
                if (t != null && !t.IsEnum)
                    throw new ArgumentException("This type is not an enumeration: " + t.FullName, nameof(enumTypes));
            }

            allEnums = new Dictionary<E, Array>();
            for (int i = 0; i < len; i++) {
                Type t = enumTypes[i];
                E e = (E)values.GetValue(i);
                if (t == null)
                    allEnums.Add(e, new object[0]);
                else
                    allEnums.Add(e, Enum.GetValues(t));
            }
            
            allItems = new Dictionary<E, object[]>();
            foreach (var kv in allEnums) {
                var v = kv.Value;
                if (v.Length == 0)
                    allItems[kv.Key] = (object[])v;
                else {
                    //allItems[kv.Key] = ((IEnumerable<T>)v).Select(v => new EnumItem<T>(v)).ToArray();

                    var arrayType = v.GetType();
                    var iEnumType = arrayType.GetInterfaces().First(t =>
                        t.IsGenericType && t.FullName.StartsWith(IENUMERABLE)
                    );
                    var innerType = iEnumType.GetGenericArguments()[0]; //T of IEnumerable<T>

                    var veiType = eiType.MakeGenericType(innerType);
                    var veiConst = veiType.GetConstructor(new[] { innerType }); //new EnumItem<T>

                    /*var castThis = cast.MakeGenericMethod(innerType); //.Cast<T>
                    var ienumerated = castThis.Invoke(null, new object[] { v }); //Array to IEnumerable<T>
                    
                    var selectVei = select.MakeGenericMethod(innerType, veiType); //.Select
                    var selected = selectVei.Invoke(null, new object[] { ienumerated,
                        (Func<dynamic, object>)(v => veiConst.Invoke(new[] { v }))
                    });

                    var toArrayVei = toArray.MakeGenericMethod(veiType); //EnumItem<T>[]
                    var array = toArrayVei.Invoke(selected, new object[0]);*/

                    int len2 = v.Length;
                    var converted = new object[len2];
                    for (int i = 0; i < len2; i++) {
                        var ei = veiConst.Invoke(new object[] { v.GetValue(i) });
                        converted[i] = ei;
                    }

                    allItems[kv.Key] = converted;
                }
            }
        }
    }

    public class EnumCombo<E> : ComboBox where E : Enum {
        readonly EnumConfig<E> config;
        public E Enums {
            get => _enum;
            set {
                if (value.Equals(_enum))
                    return;
                _enum = value;
                loadItems();
            }
        }
        E _enum;
        
        public EnumCombo(EnumConfig<E> config) {
            this.config = config;
            loadItems();
        }

        void loadItems() {
            Items.Clear();
            if (config.allItems == null)
                return;
            Items.AddRange(config.allItems[_enum]);
            Width = Items.GetMaxWidth(Font) + 20;
            DropDownHeight = Items.Count * ItemHeight + 20;
            if (Items.Count > 0)
                SelectedIndex = 0;
        }
    }
}
