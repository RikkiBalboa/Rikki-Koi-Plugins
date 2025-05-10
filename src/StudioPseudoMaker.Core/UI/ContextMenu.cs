using System;
using System.Collections.Generic;
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
        private static GameObject spacerTemplate;

        private static ContextMenu instance;
        private static List<GameObject> buttons = new List<GameObject>();

        private void Awake()
        {
            instance = this;
            buttonTemplate = gameObject.GetComponentInChildren<Button>();
            spacerTemplate = gameObject.transform.Find("SpacerTemplate").gameObject;

            buttonTemplate.gameObject.SetActive(false);
            spacerTemplate.gameObject.SetActive(false);
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
                Destroy(button);
            buttons.Clear();

            foreach (var option in options)
            {

                if (option.Value == null)
                {
                    var spacer = Instantiate(spacerTemplate, spacerTemplate.transform.parent);
                    spacer.SetActive(true);
                    buttons.Add(spacer);
                }
                else
                {
                    var button = Instantiate(buttonTemplate, buttonTemplate.transform.parent);
                    button.GetComponentInChildren<Text>().text = option.Key;
                    button.onClick.AddListener(() => option.Value());
                    button.gameObject.SetActive(true);
                    buttons.Add(button.gameObject);
                }
            }

            instance.transform.position = new Vector2(eventData.position.x + 3, eventData.position.y + 3);
            instance.gameObject.SetActive(true);
        }
    }
}
