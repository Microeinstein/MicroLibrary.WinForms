using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Micro.WinForms {
    public static class Core {
        const int WM_SETREDRAW = 11;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int wMsg, bool wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg, UIntPtr wParam, string lParam, uint fuFlags, uint uTimeout, out UIntPtr lpdwResult);

        public static void SuspendDrawing(this Control parent) {
            SendMessage(parent.Handle, WM_SETREDRAW, false, 0);
        }
        public static void ResumeDrawing(this Control parent) {
            SendMessage(parent.Handle, WM_SETREDRAW, true, 0);
            parent.Refresh();
        }

        public static T[] A<T>(params T[] items) => items;
        public static void CopyTo<T>(T[] target, params T[] items) {
            if (items.Length > target.Length)
                throw new ArgumentOutOfRangeException();
            for (int i = 0; i < items.Length; i++)
                target[i] = items[i];
        }
        public static void CopyFrom<T>(T[] target, params Action<T>[] items) {
            if (items.Length > target.Length)
                throw new ArgumentOutOfRangeException();
            for (int i = 0; i < items.Length; i++)
                items[i]?.Invoke(target[i]);
        }
        public static T[] ToArray<T>(this ICollection items) {
            var ret = new T[items.Count];
            items.CopyTo(ret, 0);
            return ret;
        }
        public static Size GetSize(this string text, Font f)
           => TextRenderer.MeasureText(text, f);
        public static int GetMaxWidth(this ICollection c, Font f)
            => c.ToArray<object>().Concat(new[] { "042" }).Select(i => i.ToString().GetSize(f).Width).Max();
        public static void GetLimits(this TypeCode tc, out object min, out object max) {
            var type = Convert.ChangeType(0, tc).GetType();
            min = type.GetField("MinValue").GetValue(null);
            max = type.GetField("MaxValue").GetValue(null);
        }
        public static bool TryChangeType<T>(object v, TypeCode c, out T r) {
            try {
                r = (T)Convert.ChangeType(v, c);
                return true;
            } catch (Exception) {
                r = default(T);
                return false;
            }
        }
    }
}
