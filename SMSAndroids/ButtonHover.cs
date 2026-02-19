using System;
using GameCreator;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Common.Audio;
using GameCreator.Runtime.Common.UnityUI;
using GameCreator.Runtime.Dialogue;
using GameCreator.Runtime.Dialogue.UnityUI;
using GameCreator.Runtime.Variables;
using GameCreator.Runtime.VisualScripting;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;

namespace SMSAndroidsCore
{
    public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private bool mouseOver;
        private UnityEngine.UI.Image image;
        public float fadeSpeed = 8f; // Adjust for faster/slower fade

        public void OnPointerEnter(PointerEventData pointerEventData)
        {
            mouseOver = true;
        }
        public void OnPointerExit(PointerEventData pointerEventData)
        {
            mouseOver = false;
        }
        public void Start()
        {
            image = transform.GetChild(2).gameObject.GetComponent<UnityEngine.UI.Image>();
        }
        public void OnEnable()
        {
            mouseOver = false;
            if (image != null)
            {
                Color c = image.color;
                c.a = 0f;
                image.color = c;
            }
        }
        public void Update()
        {
            if (image == null) return;
            Color c = image.color;
            float targetAlpha = mouseOver ? 1f : 0f;
            c.a = Mathf.MoveTowards(c.a, targetAlpha, fadeSpeed * Time.deltaTime);
            image.color = c;
        }
    }
}
