using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EnhancedUI.EnhancedScroller;
using UnityEngine.Pool;

namespace UnityEngine.UI
{
    [AddComponentMenu("MyLayout/My Grid Layout Group", 152)]
    public class MyGridLayoutGroup : GridLayoutGroup
    {
        public IGridScrollerDelegate Delegate;

        /// <summary>
        /// Gets the number of cells in a list of data
        /// </summary>
        /// <returns></returns>
        protected virtual int GetNumberOfCells()
        {
            if (Delegate == null)
            {
                return transform.childCount;
            }
            return Delegate.GetNumberOfCells();
        }

        /// <summary>
        /// Gets the cell view that should be used for the data index. Your implementation
        /// of this function should request a new cell from the scroller so that it can
        /// properly recycle old cells.
        /// </summary>
        /// <param name="dataIndex"></param>
        /// <param name="cellIndex"></param>
        /// <returns></returns>
        protected virtual RectTransform GetCellView(int dataIndex, int cellIndex)
        {
            if (Delegate != null)
            {
                return Delegate.GetCellView(dataIndex, cellIndex);
            }
            else
            {
                if (rectChildren.Count > 0)
                {
                    return GameObject.Instantiate(rectChildren[0], this.transform);
                }

                return null;
            }
        }

        public virtual void LayoutGroup_CalculateLayoutInputHorizontal()
        {
            rectChildren.Clear();
            var toIgnoreList = ListPool<Component>.Get();
            for (int i = 0; i < rectTransform.childCount; i++)
            {
                var rect = rectTransform.GetChild(i) as RectTransform;
                if (rect == null || !rect.gameObject.activeInHierarchy)
                    continue;

                rect.GetComponents(typeof(ILayoutIgnorer), toIgnoreList);

                if (toIgnoreList.Count == 0)
                {
                    rectChildren.Add(rect);
                    continue;
                }

                for (int j = 0; j < toIgnoreList.Count; j++)
                {
                    var ignorer = (ILayoutIgnorer)toIgnoreList[j];
                    if (!ignorer.ignoreLayout)
                    {
                        rectChildren.Add(rect);
                        break;
                    }
                }
            }
            ListPool<Component>.Release(toIgnoreList);
            m_Tracker.Clear();
        }

        /// <summary>
        /// Called by the layout system to calculate the horizontal layout size.
        /// Also see ILayoutElement
        /// </summary>
        public override void CalculateLayoutInputHorizontal()
        {
            LayoutGroup_CalculateLayoutInputHorizontal();

            int minColumns = 0;
            int preferredColumns = 0;
            if (m_Constraint == Constraint.FixedColumnCount)
            {
                minColumns = preferredColumns = m_ConstraintCount;
            }
            else if (m_Constraint == Constraint.FixedRowCount)
            {
                minColumns = preferredColumns = Mathf.CeilToInt(GetNumberOfCells() / (float)m_ConstraintCount - 0.001f);
            }
            else
            {
                minColumns = 1;
                preferredColumns = Mathf.CeilToInt(Mathf.Sqrt(GetNumberOfCells()));
            }

            SetLayoutInputForAxis(
                padding.horizontal + (cellSize.x + spacing.x) * minColumns - spacing.x,
                padding.horizontal + (cellSize.x + spacing.x) * preferredColumns - spacing.x,
                -1, 0);
        }

        /// <summary>
        /// Called by the layout system to calculate the vertical layout size.
        /// Also see ILayoutElement
        /// </summary>
        public override void CalculateLayoutInputVertical()
        {
            int minRows = 0;
            if (m_Constraint == Constraint.FixedColumnCount)
            {
                minRows = Mathf.CeilToInt(GetNumberOfCells() / (float)m_ConstraintCount - 0.001f);
            }
            else if (m_Constraint == Constraint.FixedRowCount)
            {
                minRows = m_ConstraintCount;
            }
            else
            {
                float width = rectTransform.rect.width;
                int cellCountX = Mathf.Max(1, Mathf.FloorToInt((width - padding.horizontal + spacing.x + 0.001f) / (cellSize.x + spacing.x)));
                minRows = Mathf.CeilToInt(GetNumberOfCells() / (float)cellCountX);
            }

            float minSpace = padding.vertical + (cellSize.y + spacing.y) * minRows - spacing.y;
            SetLayoutInputForAxis(minSpace, minSpace, -1, 1);
        }

        /// <summary>
        /// Called by the layout system
        /// Also see ILayoutElement
        /// </summary>
        public override void SetLayoutHorizontal()
        {
            SetCellsAlongAxis(0);
        }

        /// <summary>
        /// Called by the layout system
        /// Also see ILayoutElement
        /// </summary>
        public override void SetLayoutVertical()
        {
            SetCellsAlongAxis(1);
        }

        protected RectTransform SharedSampleTransform;
        protected Dictionary<int, RectTransform> UsingDict = new();
        protected static Dictionary<int, RectTransform> UsedDict = new();
        protected static readonly List<(int i, Vector4 paras)> CellViewUpdateInfo = new();
        protected static readonly Queue<RectTransform> UnallocTransforms = new();
        protected static readonly List<int> InvalidTransformIndexes = new();
        protected static readonly List<RectTransform> DelayHideChildren = new();

        protected bool IsChildrenDirty = false;
        protected override void Awake()
        {
            base.Awake();
            IsChildrenDirty = true;
        }

        protected override void OnTransformChildrenChanged()
        {
            IsChildrenDirty = true;
            base.OnTransformChildrenChanged();
        }

        private void SetCellsAlongAxis(int axis)
        {
            // Normally a Layout Controller should only set horizontal values when invoked for the horizontal axis
            // and only vertical values when invoked for the vertical axis.
            // However, in this case we set both the horizontal and vertical position when invoked for the vertical axis.
            // Since we only set the horizontal position and not the size, it shouldn't affect children's layout,
            // and thus shouldn't break the rule that all horizontal layout must be calculated before all vertical layout.
            var rectChildrenCount = GetNumberOfCells();
            if (axis == 0)
            {
                // Only set the sizes when invoked for horizontal axis, not the positions.

                var childrenCount = rectChildren.Count;
                for (int i = 0; i < childrenCount; i++)
                {
                    RectTransform rect = rectChildren[i];

                    m_Tracker.Add(this, rect,
                        DrivenTransformProperties.Anchors |
                        DrivenTransformProperties.AnchoredPosition |
                        DrivenTransformProperties.SizeDelta);

                    rect.anchorMin = Vector2.up;
                    rect.anchorMax = Vector2.up;
                    rect.sizeDelta = cellSize;
                }
                return;
            }
            // else axis == 1 below

            var rectSize = rectTransform.rect.size;
            float width = rectSize.x;
            float height = rectSize.y;

            int cellCountX = 1;
            int cellCountY = 1;
            if (m_Constraint == Constraint.FixedColumnCount)
            {
                cellCountX = m_ConstraintCount;

                if (rectChildrenCount > cellCountX)
                    cellCountY = rectChildrenCount / cellCountX + (rectChildrenCount % cellCountX > 0 ? 1 : 0);
            }
            else if (m_Constraint == Constraint.FixedRowCount)
            {
                cellCountY = m_ConstraintCount;

                if (rectChildrenCount > cellCountY)
                    cellCountX = rectChildrenCount / cellCountY + (rectChildrenCount % cellCountY > 0 ? 1 : 0);
            }
            else
            {
                if (cellSize.x + spacing.x <= 0)
                    cellCountX = int.MaxValue;
                else
                    cellCountX = Mathf.Max(1, Mathf.FloorToInt((width - padding.horizontal + spacing.x + 0.001f) / (cellSize.x + spacing.x)));

                if (cellSize.y + spacing.y <= 0)
                    cellCountY = int.MaxValue;
                else
                    cellCountY = Mathf.Max(1, Mathf.FloorToInt((height - padding.vertical + spacing.y + 0.001f) / (cellSize.y + spacing.y)));
            }

            int cornerX = (int)startCorner % 2;
            int cornerY = (int)startCorner / 2;

            int cellsPerMainAxis, actualCellCountX, actualCellCountY;
            if (startAxis == Axis.Horizontal)
            {
                cellsPerMainAxis = cellCountX;
                actualCellCountX = Mathf.Clamp(cellCountX, 1, rectChildrenCount);

                if (m_Constraint == Constraint.FixedRowCount)
                    actualCellCountY = Mathf.Min(cellCountY, rectChildrenCount);
                else
                    actualCellCountY = Mathf.Clamp(cellCountY, 1, Mathf.CeilToInt(rectChildrenCount / (float)cellsPerMainAxis));
            }
            else
            {
                cellsPerMainAxis = cellCountY;
                actualCellCountY = Mathf.Clamp(cellCountY, 1, rectChildrenCount);

                if (m_Constraint == Constraint.FixedColumnCount)
                    actualCellCountX = Mathf.Min(cellCountX, rectChildrenCount);
                else
                    actualCellCountX = Mathf.Clamp(cellCountX, 1, Mathf.CeilToInt(rectChildrenCount / (float)cellsPerMainAxis));
            }

            Vector2 requiredSpace = new Vector2(
                actualCellCountX * cellSize.x + (actualCellCountX - 1) * spacing.x,
                actualCellCountY * cellSize.y + (actualCellCountY - 1) * spacing.y
            );
            Vector2 startOffset = new Vector2(
                GetStartOffset(0, requiredSpace.x),
                GetStartOffset(1, requiredSpace.y)
            );

            // Fixes case 1345471 - Makes sure the constraint column / row amount is always respected
            int childrenToMove = 0;
            if (rectChildrenCount > m_ConstraintCount && Mathf.CeilToInt((float)rectChildrenCount / (float)cellsPerMainAxis) < m_ConstraintCount)
            {
                childrenToMove = m_ConstraintCount - Mathf.CeilToInt((float)rectChildrenCount / (float)cellsPerMainAxis);
                childrenToMove += Mathf.FloorToInt((float)childrenToMove / ((float)cellsPerMainAxis - 1));
                if (rectChildrenCount % cellsPerMainAxis == 1)
                    childrenToMove += 1;
            }

            RectTransform sampleTrans;
            {
                if (rectChildren.Count > 0)
                {
                    if (SharedSampleTransform == null)
                    {
                        sampleTrans = rectChildren[0];
                    }
                    else
                    {
                        sampleTrans = rectChildren.FirstOrDefault(t=>t!=SharedSampleTransform);
                        if (sampleTrans == null && this.transform.childCount > 0)
                        {
                            sampleTrans = (RectTransform)this.transform.GetChild(0);
                        }
                    }
                }
                else if(this.transform.childCount > 0)
                {
                    sampleTrans = (RectTransform)this.transform.GetChild(0);
                }
                else
                {
                    sampleTrans = SharedSampleTransform;
                }

                if (SharedSampleTransform != null && SharedSampleTransform != sampleTrans)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        GameObject.DestroyImmediate(SharedSampleTransform.gameObject);
                    }
                    else
#endif
                    {
                        GameObject.Destroy(SharedSampleTransform.gameObject);
                    }
                    
                    SharedSampleTransform = null;
                }
            }
            
            if(sampleTrans == null)
            {
                if (SharedSampleTransform == null)
                {
                    sampleTrans =
                        (RectTransform)(new GameObject("$SampleTrans", typeof(RectTransform)).transform);
                    
                    sampleTrans.anchorMin = Vector2.up;
                    sampleTrans.anchorMax = Vector2.up;
                    sampleTrans.sizeDelta = cellSize;
                    SharedSampleTransform = sampleTrans;
                    sampleTrans.SetParent(this.transform, false);
                }
                else
                {
                    sampleTrans = SharedSampleTransform;
                }
            }

            // Rect clipRect=new();
            ref var clipRect = ref GetClipRect();

            // var sampleChild = (RectTransform)this.transform.GetChild(0);
            // sampleChild.sizeDelta = clipRect.size;
            // sampleChild.localPosition = clipRect.center;
            // sampleChild.gameObject.SetActive(true);
            // return;
            Func<IEnumerable<int>> iterFunc;
            {
                var p0=CalcPos(rectChildrenCount, 0, childrenToMove, cellsPerMainAxis, cornerX, actualCellCountX, cornerY, actualCellCountY, startOffset);
                var diagonalIndex = cellsPerMainAxis <= 1 ? cellsPerMainAxis : cellsPerMainAxis + 1;
                var p1=CalcPos(rectChildrenCount, diagonalIndex, childrenToMove, cellsPerMainAxis, cornerX, actualCellCountX, cornerY, actualCellCountY, startOffset);

                var linesCount = (rectChildrenCount + cellsPerMainAxis - 1) / cellsPerMainAxis;
                var iPosEnd = linesCount * cellsPerMainAxis - 1;
                var pEnd=CalcPos(iPosEnd+1, iPosEnd, childrenToMove, cellsPerMainAxis, cornerX, actualCellCountX, cornerY, actualCellCountY, startOffset);

                var ar0 = CalcTransform(sampleTrans, ref p0);
                var ar1 = CalcTransform(sampleTrans, ref p1);
                var arEnd = CalcTransform(sampleTrans, ref pEnd);
                var distance = ar1.center - ar0.center;

                // sampleTrans.localPosition = ar0.center;

                if (
                    (arEnd.xMax > ar0.xMin
                        ? (arEnd.xMax > clipRect.xMin && clipRect.xMax > ar0.xMin)
                        : (ar0.xMax > clipRect.xMin && clipRect.xMax > arEnd.xMin))
                && (arEnd.yMax > ar0.yMin
                    ? (arEnd.yMax > clipRect.yMin && clipRect.yMax > ar0.yMin)
                    : (ar0.yMax > clipRect.yMin && clipRect.yMax > arEnd.yMin))
                )
                {
                    var isHorizontal = startAxis == Axis.Horizontal;
                    var isVertical = startAxis == Axis.Vertical;

                    var sign = new Vector2(Math.Sign(ar1.x - ar0.x), Math.Sign(ar1.y - ar0.y));
                    var offset0 = clipRect.center - ar0.center - 0.5f * (clipRect.size + ar0.size) * sign;
                    var offset1 = clipRect.center - ar0.center + 0.5f * (clipRect.size + ar1.size) * sign;

                    var xCountMax =  isHorizontal? actualCellCountX-1 : int.MaxValue;
                    var yCountMax =  isVertical? actualCellCountY-1 : int.MaxValue;
                    var iOffsetX0 = distance.x==0?0:Mathf.Clamp(Mathf.FloorToInt(offset0.x / distance.x), 0, xCountMax);
                    var iOffsetX1 = distance.x==0?0:Mathf.Clamp(Mathf.CeilToInt(offset1.x / distance.x), 0, xCountMax);
                    var iOffsetY0 = distance.y==0?0:Mathf.Clamp(Mathf.FloorToInt(offset0.y / distance.y),0,yCountMax);
                    var iOffsetY1 = distance.y==0?0:Mathf.Clamp(Mathf.CeilToInt(offset1.y / distance.y),0,yCountMax);
                    IEnumerable<int> IterFunc0()
                    {
                        if (isHorizontal)
                        {
                            for (var iy = iOffsetY0; iy <= iOffsetY1; iy++)
                            {
                                for (var ix = iOffsetX0; ix <= iOffsetX1; ix++)
                                {
                                    var iPos = ix + iy * actualCellCountX;
                                    if (0 <= iPos && iPos < rectChildrenCount)
                                    {
                                        yield return iPos;
                                    }
                                }
                            }
                        }
                        else
                        {
                            for (var ix = iOffsetX0; ix <= iOffsetX1; ix++)
                            {
                                for (var iy = iOffsetY0; iy <= iOffsetY1; iy++)
                                {
                                    var iPos = iy + ix * actualCellCountY;
                                    if (0 <= iPos && iPos < rectChildrenCount)
                                    {
                                        yield return iPos;
                                    }
                                }
                            }
                        }
                    }

                    iterFunc = IterFunc0;
                }
                else
                {
                    iterFunc = null;
                }
            }
            
            UsedDict.Clear();
            CellViewUpdateInfo.Clear();
            UnallocTransforms.Clear();
            
            // clear indexes
            foreach (var item in UsingDict)
            {
                if (item.Value == null)
                {
                    InvalidTransformIndexes.Add(item.Key);
                }
            }
            foreach (var invalidIndex in InvalidTransformIndexes)
            {
                UsingDict.Remove(invalidIndex);
            }
            InvalidTransformIndexes.Clear();
            
            // collect unalloc children
            if (IsChildrenDirty)
            {
                var childCount = transform.childCount;
                for (var i = 0; i < childCount; i++)
                {
                    var child = (RectTransform)transform.GetChild(i);
                    if (child!=SharedSampleTransform && !UsingDict.ContainsValue(child))
                    {
                        UnallocTransforms.Enqueue(child);
                    }
                }
                IsChildrenDirty = false;
            }
            
            if (iterFunc != null)
            {
                // for (int i = 0; i < rectChildrenCount; i++)
                foreach (var i in iterFunc())
                {
                    var paras = CalcPos(rectChildrenCount, i, childrenToMove, cellsPerMainAxis, cornerX,
                        actualCellCountX, cornerY, actualCellCountY, startOffset);
                    SetChildAlongAxis2(sampleTrans, ref paras);

                    var isVisible = IsVisibleInternal(sampleTrans, ref clipRect);
                    if (isVisible)
                    {
                        CellViewUpdateInfo.Add((i, paras));

                        if (UsingDict.Remove(i, out var rect))
                        {
                            UsedDict.Add(i, rect);
                        }
                    }
                }
            }

            // set visible children positions
            var cellIndex = 0;
            foreach (var (dataIndex, paras) in CellViewUpdateInfo)
            {
                if (!UsedDict.TryGetValue(dataIndex, out var rectTrans))
                {
                    if (UsingDict.Count > 0)
                    {
                        var item = UsingDict.First();
                        rectTrans = item.Value;
                        UsingDict.Remove(item.Key);
                        UsedDict.Add(dataIndex, rectTrans);
                    }
                    else if (Delegate == null && UnallocTransforms.Count>0)
                    {
                        rectTrans = UnallocTransforms.Dequeue();
                        UsedDict.Add(dataIndex, rectTrans);
                    }
                    else
                    {
                        rectTrans = GetCellView(dataIndex, cellIndex);
                        UsedDict.Add(dataIndex, rectTrans);
                    }
                }

                if (syncSiblingIndex)
                {
                    rectTrans.SetSiblingIndex(cellIndex);
                }

                if (hideUnused)
                {
                    rectTrans.gameObject.SetActive(true);
                }

                var paras1 = paras;
                SetChildAlongAxis2(rectTrans, ref paras1);
                cellIndex++;
            }
            
            // merge left
            var max1 = UsingDict.Count == 0 ? 0 : UsingDict.Max(item => item.Key);
            var max2 = UsedDict.Count == 0 ? 0 : UsedDict.Max(item => item.Key);
            var max = Math.Max(max1, max2);
            foreach (var unallocTransform in UnallocTransforms)
            {
                UsingDict.Add(++max, unallocTransform);
            }
            UnallocTransforms.Clear();

            if (UsingDict.Count > 0)
            {
                // recycle left
                foreach (var item in UsingDict)
                {
                    item.Value.localPosition = InVisiblePos;
                }

                if (hideUnused)
                {
                    foreach (var item in UsingDict)
                    {
                        if (item.Value.gameObject.activeSelf)
                        {
                            DelayHideChildren.Add(item.Value);
                        }
                    }
                    
                    IEnumerator DelayHide()
                    {
                        yield return new WaitForEndOfFrame();
                        foreach (var delayHideChild in DelayHideChildren)
                        {
                            delayHideChild.gameObject.SetActive(false);
                        }

                        DelayHideChildren.Clear();
                    }

                    StartCoroutine(DelayHide());
                }
            }
            
            // merge all
            foreach (var item in UsedDict)
            {
                UsingDict.Add(item.Key,item.Value);
            }

            UsedDict.Clear();
            CellViewUpdateInfo.Clear();
        }

        private bool IsVisibleInternal(RectTransform sampleTrans, ref Rect clipRect)
        {
            return Delegate != null
                ? Delegate.IsVisible(sampleTrans)
                : IsInContainer(ref clipRect, sampleTrans);
        }

        protected Rect ScreenRect;
        private ref Rect GetClipRect()
        {
            ref Rect clipRect = ref ScreenRect;
            if (Delegate == null)
            {
                // clipRect = new();
                var res = Screen.currentResolution;
                var pos = ToVec2(this.rectTransform.localPosition);
                clipRect = new(-res.width * 0.5f-pos.x, -res.height * 0.5f-pos.y, res.width, res.height);
            }
            else
            {
                clipRect = ref Delegate.GetClipRect();
            }

            return ref clipRect;
        }

        /// <summary>
        /// 开启此选项，会使可见的节点按顺序重新紧凑排列，否则节点顺序不会改变，只改变坐标
        /// </summary>
        public bool syncSiblingIndex = false;

        public bool hideUnused = false;

        private Vector4 CalcPos(int rectChildrenCount, int i, int childrenToMove, int cellsPerMainAxis, int cornerX,
            int actualCellCountX, int cornerY, int actualCellCountY, Vector2 startOffset)
        {
            int positionX;
            int positionY;
            if (startAxis == Axis.Horizontal)
            {
                if (m_Constraint == Constraint.FixedRowCount && rectChildrenCount - i <= childrenToMove)
                {
                    positionX = 0;
                    positionY = m_ConstraintCount - (rectChildrenCount - i);
                }
                else
                {
                    positionX = i % cellsPerMainAxis;
                    positionY = i / cellsPerMainAxis;
                }
            }
            else
            {
                if (m_Constraint == Constraint.FixedColumnCount && rectChildrenCount - i <= childrenToMove)
                {
                    positionX = m_ConstraintCount - (rectChildrenCount - i);
                    positionY = 0;
                }
                else
                {
                    positionX = i / cellsPerMainAxis;
                    positionY = i % cellsPerMainAxis;
                }
            }

            if (cornerX == 1)
                positionX = actualCellCountX - 1 - positionX;
            if (cornerY == 1)
                positionY = actualCellCountY - 1 - positionY;

            var paras = new Vector4(startOffset.x + (cellSize[0] + spacing[0]) * positionX, cellSize[0],
                startOffset.y + (cellSize[1] + spacing[1]) * positionY, cellSize[1]);
            return paras;
        }

        public static readonly Vector3 InVisiblePos = new Vector3(float.MaxValue * 0.5f, float.MaxValue * 0.5f, 0);

        protected Vector2 SharedVec;
        protected void SetChildAlongAxis2(RectTransform rect, ref Vector4 paras)
        {
            if (rect == null)
                return;

            SetChildAlongAxisWithScale2(rect, ref paras);
        }
        /// <summary>
        /// Set the position and size of a child layout element along the given axis.
        /// </summary>
        /// <param name="rect">The RectTransform of the child layout element.</param>
        /// <param name="axis">The axis to set the position and size along. 0 is horizontal and 1 is vertical.</param>
        /// <param name="pos">The position from the left side or top.</param>
        /// <param name="size">The size.</param>
        protected void SetChildAlongAxis2(RectTransform rect, int axis, float pos, float size)
        {
            if (rect == null)
                return;

            SetChildAlongAxisWithScale2(rect, axis, pos, size, 1.0f);
        }

        /// <summary>
        /// Set the position and size of a child layout element along the given axis.
        /// </summary>
        /// <param name="rect">The RectTransform of the child layout element.</param>
        /// <param name="axis">The axis to set the position and size along. 0 is horizontal and 1 is vertical.</param>
        /// <param name="pos">The position from the left side or top.</param>
        /// <param name="size">The size.</param>
        /// <param name="scaleFactor"></param>
        protected void SetChildAlongAxisWithScale2(RectTransform rect, int axis, float pos, float size, float scaleFactor)
        {
            if (rect == null)
                return;

            // Inlined rect.SetInsetAndSizeFromParentEdge(...) and refactored code in order to multiply desired size by scaleFactor.
            // sizeDelta must stay the same but the size used in the calculation of the position must be scaled by the scaleFactor.

            if (rect.anchorMin != Vector2.up || rect.anchorMax != Vector2.up)
            {
                rect.anchorMin = Vector2.up;
                rect.anchorMax = Vector2.up;
            }

            Vector2 sizeDelta = rect.sizeDelta;
            sizeDelta[axis] = size;
            rect.sizeDelta = sizeDelta;

            Vector2 anchoredPosition = rect.anchoredPosition;
            anchoredPosition[axis] = (axis == 0) ? (pos + size * rect.pivot[axis] * scaleFactor) : (-pos - size * (1f - rect.pivot[axis]) * scaleFactor);
            rect.anchoredPosition = anchoredPosition;
        }

        private Rect CalcTransform(RectTransform sampleTrans, ref Vector4 paras)
        {
            // SetChildAlongAxis2(sampleTrans, 0, paras.x, paras.y);
            // SetChildAlongAxis2(sampleTrans, 1, paras.z, paras.w);
            // var rect = sampleTrans.rect;
            // var localPosition = sampleTrans.localPosition;
            // rect.center += new Vector2(localPosition.x, localPosition.y);
            // return rect;

            SetChildAlongAxisWithScale2(sampleTrans, ref paras);

            var localPosition = sampleTrans.localPosition;
            SharedVec.x = localPosition.x;
            SharedVec.y = localPosition.y;
            var rect = sampleTrans.rect;
            rect.center += SharedVec;
            return rect;
        }

        private void SetChildAlongAxisWithScale2(RectTransform sampleTrans, ref Vector4 paras)
        {
            if (sampleTrans.anchorMin != Vector2.up || sampleTrans.anchorMax != Vector2.up)
            {
                sampleTrans.anchorMin = Vector2.up;
                sampleTrans.anchorMax = Vector2.up;
            }

            SharedVec.x = paras.y;
            SharedVec.y = paras.w;
            sampleTrans.sizeDelta = SharedVec;

            var x = CalcAnchoredPositionAlongAxis(sampleTrans, 0, paras.x, paras.y);
            var y = CalcAnchoredPositionAlongAxis(sampleTrans, 1, paras.z, paras.w);
            SharedVec.x = x;
            SharedVec.y = y;
            sampleTrans.anchoredPosition = SharedVec;
        }

        private static Vector2 CalcAnchoredPosition(RectTransform rect, Vector4 paras)
        {
            var x = CalcAnchoredPositionAlongAxis(rect, 0, paras.x, paras.y);
            var y = CalcAnchoredPositionAlongAxis(rect, 1, paras.z, paras.w);
            return new Vector2(x, y);
        }
        private static float CalcAnchoredPositionAlongAxis(RectTransform rect, int axis, float pos, float size, float scaleFactor=1.0f)
        {
            var apos = (axis == 0) ? (pos + size * rect.pivot[axis] * scaleFactor) : (-pos - size * (1f - rect.pivot[axis]) * scaleFactor);
            return apos;
        }
        
        
        public Vector2 ToVec2(Vector3 pos)
        {
            return new Vector2(pos.x, pos.y);
        }

        public Rect GetRectBounds(RectTransform trans)
        {
            var rect = trans.rect;
            // rect.center = rect.center + ToVec2(transform.position) - ToVec2(_root.transform.position);
            rect.center += ToVec2(trans.localPosition);
            return rect;
        }

        protected bool IsInContainer(ref Rect clipRect, RectTransform child)
        {
            var rect = GetRectBounds(child);
            return IsInContainer(ref clipRect, ref rect);
        }

        protected bool IsInContainer(ref Rect clipRect, ref Rect rect)
        {
            var isInContainer = (rect.yMax >= clipRect.yMin && rect.yMin <= clipRect.yMax) && (rect.xMax >= clipRect.xMin && rect.xMin <= clipRect.xMax);
            return isInContainer;
        }

    }
}
