using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class MessageBox : MonoBehaviour
    {
        private static MessageBox _instance;

        public static MessageBox Instance => _instance;

        [SerializeField]
        private Canvas m_canvas;

        [SerializeField]
        private TMP_Text m_txtTitle;

        [SerializeField]
        private TMP_Text m_txtMessage;

        public string Title
        {
            get
            {
                return m_txtTitle.text;
            }
            set
            {
                m_txtTitle.text = value;
            }
        }

        public string Message
        {
            get
            {
                return m_txtMessage.text;
            }
            set
            {
                m_txtMessage.text = value;
            }
        }

        public MessageBox()
        {
            
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(base.gameObject);
            }
            else
            {
                Destroy(base.gameObject);
            }
        }

        private void Start()
        {
            base.gameObject.SetActive(false);
        }


        public void Show(object message)
        {
            this.Message = message.ToString();
            base.gameObject.SetActive(true);
            m_canvas.sortingOrder = 1;
        }
    }


}