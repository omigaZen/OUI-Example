using UnityEngine;
using UnityEngine.UI;

namespace OUI.Example
{
    public class TestGenWindow: BaseWindow
    {
        #region 脚本工具生成的代码

        private Image m_img_tile;
        private Button m_btn_Btn;
        private GameObject m_go_xx;
        private GameObject m_go_sub;
        private GameObject m_item_haha;

        public override void BindUI()
        {
            m_img_tile = FindChildComponent<Image>("m_img_tile");
            m_btn_Btn = FindChildComponent<Button>("m_btn_Btn");
            m_go_xx = FindChild("GameObject/m_go_xx").gameObject;
            m_go_sub = FindChild("GameObject/m_go_xx/m_go_sub").gameObject;
            m_item_haha = FindChild("GameObject/m_item_haha").gameObject;
            // m_btn_Btn.onClick.AddListener(OnClick_BtnBtn);
        }

        #endregion


        public override void Init()
        {
            throw new System.NotImplementedException();
        }

        public override void Release()
        {
            throw new System.NotImplementedException();
        }


    }
}