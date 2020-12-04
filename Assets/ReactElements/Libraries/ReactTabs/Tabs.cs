using System;
using System.Collections.Generic;
using System.Linq;
using ReactElements.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactElements.Libraries.ReactTabs
{
    public class Tabs : ReactElement
    {
        public string activeKey;
        public Action<string> onSelect;

        public void HandleClick(string key)
        {
            onSelect?.Invoke(key);
        }

        public override VisualElement Render()
        {
            base.Render();

            VisualElement leftSide = React.CreateElement<VisualElement>(
                elem =>
                {
                    elem.style.width = 120;
                    elem.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f);
                },
                children.Select(item =>
                {
                    Tab tab = item as Tab;
                    return React.CreateElement<Tab>(target =>
                    {
                        target.title = tab.title;
                        target.eventKey = tab.eventKey;
                        target.active = tab.eventKey == activeKey;
                        target.onClick = HandleClick;
                    });
                }).ToArray()
            );

            VisualElement rightSide = React.CreateElement<VisualElement>(
                elem =>
                {
                    elem.style.flexGrow = 1;

                    elem.style.paddingTop = 10;
                    elem.style.paddingBottom = 10;
                    elem.style.paddingLeft = 10;
                    elem.style.paddingRight = 10;
                },
                new[]
                {
                    (
                        children.First(item =>
                            {
                                Tab tab = item as Tab;
                                return tab.eventKey == activeKey;
                            }
                        ) as Tab
                    ).children.First()
                }
            );

            return Append(
                leftSide,
                rightSide
            );
        }
    }
}