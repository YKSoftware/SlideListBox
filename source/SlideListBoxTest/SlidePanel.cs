namespace SlideListBoxTest
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Media.Animation;

    /// <summary>
    /// カレントアイテムをスライド表示するパネルコントロールを表します。
    /// </summary>
    internal class SlidePanel : Panel
    {
        #region コンストラクタ

        /// <summary>
        /// 新しいインスタンスを生成します。
        /// </summary>
        public SlidePanel()
        {
            this.AnimationTime = 300;
            this.RenderTransform = new TranslateTransform();

            this.Loaded += OnLoaded;
            this.LayoutUpdated += OnLayoutUpdated;
            this.SizeChanged += OnSizeChanged;
        }

        #endregion コンストラクタ

        #region TargetIndex 依存関係プロパティ

        /// <summary>
        /// TargetIndex 依存関係プロパティの定義
        /// </summary>
        public static readonly DependencyProperty TargetIndexProperty = DependencyProperty.Register("TargetIndex", typeof(int), typeof(SlidePanel), new PropertyMetadata(0, OnTargetIndexPropertyChanged));

        /// <summary>
        /// 表示するアイテムのインデックスを取得または設定します。
        /// </summary>
        public int TargetIndex
        {
            get { return (int)GetValue(TargetIndexProperty); }
            set { SetValue(TargetIndexProperty, value); }
        }

        /// <summary>
        /// TargetIndex プロパティ変更イベントハンドラ
        /// </summary>
        /// <param name="sender">イベント発行元</param>
        /// <param name="e">イベント引数</param>
        private static void OnTargetIndexPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as SlidePanel).StartAnimation();
        }

        #endregion TargetIndex 依存関係プロパティ

        #region AnimationTime プロパティ

        /// <summary>
        /// アニメーション時間をミリ秒単位で取得または設定します。
        /// </summary>
        public int AnimationTime { get; set; }

        #endregion AnimationTime プロパティ

        #region イベントハンドラ

        /// <summary>
        /// ロードイベントハンドラ
        /// </summary>
        /// <param name="sender">イベント発行元</param>
        /// <param name="e">イベント引数</param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var listbox = FindAncestor<ListBox>(this);
            if (listbox != null)
            {
                var binding = new Binding()
                {
                    Path = new PropertyPath("SelectedIndex", null),
                    Source = listbox,
                };
                this.SetBinding(TargetIndexProperty, binding);
            }
        }

        /// <summary>
        /// レイアウト変更イベントハンドラ
        /// </summary>
        /// <param name="sender">イベント発行元</param>
        /// <param name="e">イベント引数</param>
        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            if (this._visualParent != null)
            {
                if (this._lastVisualParentSize != this._visualParent.RenderSize)
                {
                    InvalidateArrange();
                }
            }
        }

        /// <summary>
        /// サイズ変更イベントハンドラ
        /// </summary>
        /// <param name="sender">イベント発行元</param>
        /// <param name="e">イベント引数</param>
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            StartAnimation();
        }

        #endregion イベントハンドラ

        #region override

        /// <summary>
        /// サイズ計測のオーバーライド
        /// </summary>
        /// <param name="availableSize">子要素に割り当てることのできる使用可能サイズを指定します。</param>
        /// <returns>レイアウト過程でこの要素が必要であると判断するサイズを返します。</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            var maxSize = new Size();
            foreach (FrameworkElement child in this.InternalChildren)
            {
                child.Measure(availableSize);
                maxSize.Width = Math.Max(maxSize.Width, child.DesiredSize.Width);
                maxSize.Height = Math.Max(maxSize.Height, child.DesiredSize.Height);
            }

            var size = (double.IsInfinity(availableSize.Width) || double.IsInfinity(availableSize.Height)) ?
                new Size(maxSize.Width * this.InternalChildren.Count, maxSize.Height) :
                new Size(availableSize.Width * this.InternalChildren.Count, availableSize.Height);

            return size;
        }

        /// <summary>
        /// 子要素配置のオーバーライド
        /// </summary>
        /// <param name="finalSize">要素自体と子を配置するために使用する親の末尾の領域を指定します。</param>
        /// <returns>実際に使用されたサイズを返します。</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            base.ArrangeOverride(finalSize);

            if (this._visualParent != null)
            {
                this._lastVisualParentSize = this._visualParent.RenderSize;
            }
            else
            {
                this._lastVisualParentSize = finalSize;
            }

            for (var i = 0; i < this.InternalChildren.Count; i++)
            {
                var rect = new Rect(new Point(this._lastVisualParentSize.Width * i, 0), this._lastVisualParentSize);
                this.InternalChildren[i].Arrange(rect);
            }

            return new Size(this._lastVisualParentSize.Width * this.InternalChildren.Count, this._lastVisualParentSize.Height);
        }

        /// <summary>
        /// ビジュアルツリーの親要素変更イベントハンドラ
        /// </summary>
        /// <param name="oldParent">変更前の親要素を指定します。</param>
        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            this._visualParent = this.VisualParent as FrameworkElement;
        }

        /// <summary>
        /// クリッピングマスクのジオメトリを返します。
        /// </summary>
        /// <param name="layoutSlotSize">ビジュアルプレゼンテーションを行う要素の部分のサイズを指定します。</param>
        /// <returns>クリッピングジオメトリを返します。</returns>
        protected override Geometry GetLayoutClip(Size layoutSlotSize)
        {
            return null;
        }

        #endregion override

        #region ヘルパ

        /// <summary>
        /// アニメーションを開始します。
        /// </summary>
        private void StartAnimation()
        {
            var slideAnimation = new DoubleAnimation()
            {
                To = -this._lastVisualParentSize.Width * this.TargetIndex,
                Duration = TimeSpan.FromMilliseconds((double)AnimationTime),
                AccelerationRatio = 0.7,
                DecelerationRatio = 0.3,
            };
            Storyboard.SetTarget(slideAnimation, this);
            Storyboard.SetTargetProperty(slideAnimation, new PropertyPath("RenderTransform.(TranslateTransform.X)"));
            var storyboard = new Storyboard();
            storyboard.Children.Add(slideAnimation);
            this.BeginStoryboard(storyboard);
        }

        /// <summary>
        /// 指定された型の親要素を返します。
        /// </summary>
        /// <typeparam name="T">親要素の型を指定します。</typeparam>
        /// <param name="element">起点となる FrameworkElement オブジェクトを指定します。</param>
        /// <returns>指定された型の親要素を返します。存在しない場合は null を返します。</returns>
        private FrameworkElement FindAncestor<T>(FrameworkElement element)
            where T : FrameworkElement
        {
            do
            {
                element = (element.Parent ?? element.TemplatedParent) as FrameworkElement;
                if (element is T) return element as T;
            } while (element != null);
            return null;
        }

        #endregion ヘルパ

        #region private フィールド

        /// <summary>
        /// ビジュアルツリーの親要素
        /// </summary>
        private FrameworkElement _visualParent;

        /// <summary>
        /// ビジュアルツリーの親要素の描画サイズ
        /// </summary>
        private Size _lastVisualParentSize;

        #endregion private フィールド
    }
}
