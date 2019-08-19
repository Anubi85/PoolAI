using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using PoolAI.SDK;
using System.Linq;

namespace PoolAI.Controls
{
    partial class ComponentSelector<T, TMetaData> : UserControl where T : class where TMetaData : IMetadataBase
    {
        #region Constants

        private const int c_MaxColumns = 5;
        private const int c_MaxRows = 5;

        #endregion

        #region Fields

        [ImportMany]
        private List<ExportFactory<T, TMetaData>> m_ComponentFactories;
        private string m_CatalogPath;

        #endregion

        #region Constructor

        public ComponentSelector(string catalogPath)
        {
            InitializeComponent();
            m_CatalogPath = catalogPath;
        }

        #endregion

        #region Event Handlers

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //load available components
            if (!DesignMode)
            {
                new CompositionContainer(new DirectoryCatalog(m_CatalogPath)).ComposeParts(this);
            }
            else
            {
                m_ComponentFactories = new List<ExportFactory<T, TMetaData>>();
            }
            int colIdx = 0;
            int rowIdx = 0;
            if (m_ComponentFactories.Count == 1)
            {
                //only one component, select it!
                ComponentSelected?.Invoke(this, m_ComponentFactories.First());
                return;
            }
            foreach (ExportFactory<T, TMetaData> factory in m_ComponentFactories)
            {
                if (colIdx != 0 && rowIdx == 0)
                {
                    MainLayout.ColumnCount++;
                    MainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent));
                    //adjust width
                    foreach (ColumnStyle column in MainLayout.ColumnStyles)
                    {
                        column.Width = 1.0f / MainLayout.ColumnCount;
                    }
                }
                Button btn = new Button
                {
                    Text = factory.Metadata.Name,
                    Anchor = AnchorStyles.None,
                    Width = 80,
                    Height = 50,
                    Tag = factory
                };
                btn.Click += OnClick;
                MainLayout.Controls.Add(btn, colIdx, rowIdx);
                if (++colIdx >= c_MaxColumns)
                {
                    colIdx = 0;
                    //TODO: Handle pages!
                    rowIdx++;
                    MainLayout.RowCount++;
                    MainLayout.RowStyles.Add(new RowStyle(SizeType.Percent));
                    //adjust height
                    foreach (RowStyle row in MainLayout.RowStyles)
                    {
                        row.Height = 1.0f / MainLayout.RowCount;
                    }
                }
            }
        }
        private void OnClick(object sender, EventArgs e)
        {
            ComponentSelected?.Invoke(this, (sender as Button).Tag as ExportFactory<T, TMetaData>);
        }

        #endregion

        #region Events

        public event Action<ComponentSelector<T, TMetaData>, ExportFactory<T, TMetaData>> ComponentSelected;

        #endregion
    }
}
