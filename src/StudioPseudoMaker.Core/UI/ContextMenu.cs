using System;
using System.Collections.Generic;
using System.Text;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UniRx;

namespace PseudoMaker.UI
{
    public class ContextMenu : MonoBehaviour
    {
        private static Button buttonTemplate;
        private static ContextMenu instance;
        private static List<Button> buttons = new List<Button>();

        private void Awake()
        {
            instance = this;
            buttonTemplate = gameObject.GetComponentInChildren<Button>();

            buttonTemplate.gameObject.SetActive(false);
            gameObject.SetActive(false);

            transform.UpdateAsObservable().Subscribe(_ =>
            {
                if (gameObject && Input.GetMouseButtonUp(0))
                    gameObject.SetActive(false);
            });
        }

        public static void OpenContextMenu(PointerEventData eventData, Dictionary<string, Action> options)
        {
            foreach (var button in buttons)
                Destroy(button.gameObject);
            buttons.Clear();

            foreach (var option in options)
            {
                var button = Instantiate(buttonTemplate, buttonTemplate.transform.parent);
                button.GetComponentInChildren<Text>().text = option.Key;
                button.onClick.AddListener(() => option.Value());
                button.gameObject.SetActive(true);
                buttons.Add(button);
            }

            instance.transform.position = new Vector2(eventData.position.x + 3, eventData.position.y + 3);
            instance.gameObject.SetActive(true);
            PseudoMaker.Logger.LogInfo(eventData.position);
            PseudoMaker.Logger.LogInfo(instance.transform.position);
        }
    }
}
