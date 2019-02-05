using System;
using System.Windows;
using System.Windows.Media;

namespace aframe
{
    public static class DependencyObjectExtensions
    {
        /// <summary>
        /// 子オブジェクトに対してデリゲートを実行する
        /// </summary>
        /// <param name="obj">this : DependencyObject</param>
        /// <param name="action">デリゲート : Action</param>
        public static void Walk(
            this DependencyObject obj,
            Action<DependencyObject> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException();
            }

            WalkCore(obj, action);
        }

        /// <summary>
        /// WalkInChildrenメソッドの本体
        /// </summary>
        /// <param name="obj">DependencyObject</param>
        /// <param name="action">Action</param>
        private static void WalkCore(
            DependencyObject obj,
            Action<DependencyObject> action)
        {
            var count = VisualTreeHelper.GetChildrenCount(obj);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is DependencyObject)
                {
                    action(child as DependencyObject);
                    WalkCore(child as DependencyObject, action);
                }
            }
        }
    }
}
