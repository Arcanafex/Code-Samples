using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Spaces.Core
{
    public class DisplayGridWidget : Widget
    {
        //public int rows;
        public int columns;
        //public GameObject[,] cells;
        public float spacing;
        //public HorizontalLayoutGroup horizontalPanel;

        public FeedDisplayAdapterWidget cellPrefab;
        //public VerticalLayoutGroup verticalPanelPrefab;

        void Start()
        {
            PopulateDisplay();
        }

        public void PopulateDisplay()
        {
            for (int c = 0; c < columns; c++)
            {
                GameObject go = Instantiate(cellPrefab.gameObject);
                go.transform.SetParent(transform);
                go.transform.localPosition = new Vector3((c - (columns - 1)/2f) * spacing, 0, 0);

                go.SetActive(true);
            }
        }

        void Update()
        {
            //if (cells == null)
            //{
            //    cells = new GameObject[rows, columns];
            //}
            //else
            //{
            //    if (cells.GetLength(0) != rows || cells.GetLength(1) != columns)
            //    {
            //        GameObject[,] oldCells = cells;
            //        cells = new GameObject[rows, columns];

            //        for (int r = 0; r < rows; r++)
            //        {
            //            for (int c = 0; c < columns; c++)
            //            {
            //                if (r < oldCells.GetLength(0) && c < oldCells.GetLength(1))
            //                    cells[r, c] = oldCells[r, c];
            //                else
            //                    cells[r, c] = cellPrefab ? Instantiate(cellPrefab) : null;
            //            }
            //        }
            //    }
            //}
        }

        //void UpdateGrid ()
        //{
        //    if (horizontalPanel.transform.childCount < columns)
        //        while (horizontalPanel.transform.childCount < columns)
        //        {
        //            GameObject colObj = Instantiate(verticalPanelPrefab.gameObject);
        //            colObj.transform.SetParent(horizontalPanel.transform);
        //        }
        //    else
        //    {
        //        while (horizontalPanel.transform.childCount > columns)
        //        {
        //            Destroy(horizontalPanel.transform.GetChild(horizontalPanel.transform.childCount - 1));
        //        }
        //    }

        //    for (int r = 0; r < rows; r++)
        //    {
        //        for (int c = 0; c < columns; c++)
        //        {
        //            if (cells[r, c])
        //            {
        //                cells[r, c].transform.SetParent(horizontalPanel.transform.GetChild(c));
        //            }
        //        }
        //    }
        //}

    }
}