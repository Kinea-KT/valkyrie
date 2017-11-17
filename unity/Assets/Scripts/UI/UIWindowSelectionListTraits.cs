﻿using UnityEngine;
using Assets.Scripts.Content;
using System.Collections.Generic;

namespace Assets.Scripts.UI
{
    public class UIWindowSelectionListTraits : UIWindowSelectionList
    {
        protected List<TraitGroup> traitData = new List<TraitGroup>();

        protected SortedList<int, SelectionItemTraits> traitItems = new SortedList<int, SelectionItemTraits>();
        protected SortedList<string, SelectionItemTraits> alphaTraitItems = new SortedList<string, SelectionItemTraits>();

        protected float scrollPos = 0;

        protected UIElementScrollVertical traitScrollArea;

        public UIWindowSelectionListTraits(UnityEngine.Events.UnityAction<string> call, string title = "") : base(call, title)
        {
        }

        public UIWindowSelectionListTraits(UnityEngine.Events.UnityAction<string> call, StringKey title) : base(call, title)
        {
        }

        override public void Draw()
        {
            foreach (SelectionItemTraits item in traitItems)
            {
                foreach (string category in item.GetTraits().Keys)
                {
                    bool found = false;
                    foreach (TraitGroup tg in traitData)
                    {
                        if (tg.GetName().Equals(category))
                        {
                            found = true;
                            tg.AddTraits(item);
                        }
                    }

                    if (!found)
                    {
                        TraitGroup tg = new TraitGroup(category);
                        tg.AddTraits(item);
                        traitData.Add(tg);
                    }
                }
            }

            foreach (SelectionItemTraits item in traitItems)
            {
                foreach (TraitGroup tg in traitData)
                {
                    tg.AddItem(item);
                }
            }

            Update();
        }

        protected void Update()
        {
            bool resetScroll = false;
            if (traitScrollArea == null)
            {
                resetScroll = true;
            }
            else
            {
                scrollPos = traitScrollArea.GetScrollPosition();
            }


            // Border
            UIElement ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-18), 0, 36, 30);
            new UIElementBorder(ui);

            // Title
            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-10), 0, 20, 1);
            ui.SetText(_title);

            traitScrollArea = new UIElementScrollVertical();
            traitScrollArea.SetLocation(UIScaler.GetHCenter(-17.5f), 2, 13, 25);
            new UIElementBorder(traitScrollArea);

            float offset = 0;
            foreach (TraitGroup tg in traitData)
            {
                ui = new UIElement(traitScrollArea.GetScrollTransform());
                ui.SetLocation(0, offset, 12, 1);
                ui.SetText(tg.GetName(), Color.black);
                ui.SetTextAlignment(TextAnchor.MiddleLeft);
                ui.SetBGColor(new Color(0.5f, 1, 0.5f));
                offset += 1.05f;

                bool noneSelected = tg.NoneSelected();

                foreach (string s in tg.traits.Keys)
                {
                    TraitGroup tmpGroup = tg;
                    string tmpTrait = s;
                    ui = new UIElement(traitScrollArea.GetScrollTransform());
                    ui.SetLocation(0, offset, 11, 1);
                    if (tg.traits[s].selected)
                    {
                        ui.SetBGColor(Color.white);
                        ui.SetButton(delegate { SelectTrait(tmpGroup, tmpTrait); });
                    }
                    else
                    {
                        int itemCount = 0;
                        foreach (SelectionItemTraits item in tg.traits[s].items)
                        {
                            bool display = true;
                            foreach (TraitGroup g in traitData)
                            {
                                display &= g.ActiveItem(item);
                            }
                            if (display) itemCount++;
                        }
                        if (itemCount > 0)
                        {
                            if (noneSelected)
                            {
                                ui.SetBGColor(Color.white);
                            }
                            else
                            {
                                ui.SetBGColor(Color.grey);
                            }
                            ui.SetButton(delegate { SelectTrait(tmpGroup, tmpTrait); });
                        }
                        else
                        {
                            ui.SetBGColor(new Color(0.5f, 0, 0));
                        }
                    }
                    ui.SetText(s, Color.black);

                    // Strikethrough
                    if (tg.traits[s].excluded)
                    {
                        ui = new UIElement(traitScrollArea.GetScrollTransform());
                        ui.SetLocation(0.2f, offset + 0.5f, 10.6f, 0.06f);
                        ui.SetBGColor(Color.black);
                        ui.SetButton(delegate { SelectTrait(tmpGroup, tmpTrait); });
                    }

                    // Exclude
                    ui = new UIElement(traitScrollArea.GetScrollTransform());
                    ui.SetLocation(11, offset, 1, 1);
                    ui.SetBGColor(Color.red);
                    ui.SetText("X", Color.black);
                    ui.SetButton(delegate { ExcludeTrait(tmpGroup, tmpTrait); });

                    offset += 1.05f;
                }
                offset += 1.05f;
            }
            traitScrollArea.SetScrollSize(offset);
            if (!resetScroll)
            {
                traitScrollArea.SetScrollPosition(scrollPos);
            }

            DrawItemList();

            // Cancel button
            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-4.5f), 28, 9, 1);
            ui.SetBGColor(new Color(0.03f, 0.0f, 0f));
            ui.SetText(CommonStringKeys.CANCEL);
            ui.SetButton(delegate { Destroyer.Dialog(); });
            new UIElementBorder(ui);
        }

        protected virtual void DrawItemList()
        {
            UIElementScrollVertical itemScrollArea = new UIElementScrollVertical();
            itemScrollArea.SetLocation(UIScaler.GetHCenter(-3.5f), 2, 21, 25);
            new UIElementBorder(itemScrollArea);

            SortedList toDisplay = traitItems;
            if (alphaSort)
            {
                toDisplay = alphaTraitItems;
            }
            if (reverseSort)
            {
                toDisplay = toDisplay.Reverse();
            }

            float offset = 0;
            foreach (SelectionItemTraits item in toDisplay)
            {
                bool display = true;
                foreach (TraitGroup tg in traitData)
                {
                    display &= tg.ActiveItem(item);
                }

                if (!display) continue;

                offset = DrawItem(item, itemScrollArea.GetScrollTransform(), offset);
            }
            itemScrollArea.SetScrollSize(offset);
        }

        protected virtual float DrawItem(SelectionItemTraits item, Transform transform, float offset)
        {
            string key = item.GetKey();
            UIElement ui = new UIElement(transform);
            ui.SetLocation(0, offset, 20, 1);
            if (key != null)
            {
                ui.SetButton(delegate { SelectItem(key); });
            }
            ui.SetBGColor(item.GetColor());
            ui.SetText(item.GetDisplay(), Color.black);
            return offset + 1.05f;
        }

        protected void SelectTrait(TraitGroup group, string trait)
        {
            if (!traits.ContainsKey(trait)) return;

            group.traits[trait].selected = !group.traits[trait].selected;
            group.traits[trait].excluded = false;
            Update();
        }

        protected void ExcludeTrait(TraitGroup group, string trait)
        {
            if (!traits.ContainsKey(trait)) return;

            group.traits[trait].excluded = !group.traits[trait].excluded;
            group.traits[trait].selected = false;
            Update();
        }

        public void AddItem(StringKey stringKey, Dictionary<string, IEnumerable<string>> traits)
        {
            AddItem(new SelectionItemTraits(stringKey.Translate(), stringKey.key, traits));
        }

        public void AddItem(StringKey stringKey, Dictionary<string, IEnumerable<string>> traits, Color color)
        {
            AddItem(new SelectionItemTraits(stringKey.Translate(), stringKey.key, traits), color);
        }

        public void AddItem(string item, Dictionary<string, IEnumerable<string>> traits)
        {
            AddItem(new SelectionItemTraits(item, item, traits));
        }

        public void AddItem(string item, Dictionary<string, IEnumerable<string>> traits, Color color)
        {
            AddItem(new SelectionItemTraits(item, item, traits), color);
        }

        public void AddItem(string display, string key, Dictionary<string, IEnumerable<string>> traits)
        {
            AddItem(new SelectionItemTraits(display, key, traits), color);
        }

        public void AddItem(string display, string key, Dictionary<string, IEnumerable<string>> traits, Color color)
        {
            AddItem(new SelectionItemTraits(display, key, traits), color);
        }

        public void AddItem(QuestData.QuestComponent qc)
        {
            Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();

            traits.Add(new StringKey("val", "TYPE").Translate(), new string[] { new StringKey("val", qc.typeDynamic.ToUpper()).Translate() });
            traits.Add(new StringKey("val", "SOURCE").Translate(), new string[] { qc.source });

            AddItem(new SelectionItemTraits(qc.sectionName, qc.sectionName, traits));
        }

        public void AddItem(GenericData component)
        {
            AddItem(CreateItem(component));
        }

        public void AddItem(GenericData component, Color color)
        {
            AddItem(CreateItem(component), color);
        }

        override void AddItem(SelectionItem item)
        {
            if (item is SelectionItemTraits)
            {
                traitItems.Add(traitItems.Count, new SelectionItemTraits(item));
                alphaTraitItems.Add(item.GetDisplay(), new SelectionItemTraits(item));
            }
            else
            {
                traitItems.Add(itemIndex, item);
                alphaTraitItems.Add(item.GetDisplay(), item);
            }
        }

        protected void AddItem(SelectionItem item, Color color)
        {
            item.SetColor(color);
            AddItem(item);
        }

        protected SelectionItemTraits CreateItem(GenericData component)
        {
            Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();

            List<string> sets = new List<string>();
            foreach (string s in component.sets)
            {
                if (s.Length == 0)
                {
                    sets.Add(new StringKey("val", "base").Translate());
                }
                else
                {
                    sets.Add(new StringKey("val", s).Translate());
                }
            }
            traits.Add(new StringKey("val", "SOURCE").Translate(), sets);

            List<string> traitlocal = new List<string>();
            foreach (string s in component.traits)
            {
                traitlocal.Add(new StringKey("val", s).Translate());
            }
            traits.Add(new StringKey("val", "TRAITS").Translate(), traitlocal);

            return new SelectionItemTraits(component.name.Translate(), component.sectionName, traits);
        }

        public void AddNewComponentItem(string type)
        {
            Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();

            traits.Add(new StringKey("val", "TYPE").Translate(), new string[] { new StringKey("val", type.ToUpper()).Translate() });
            traits.Add(new StringKey("val", "SOURCE").Translate(), new string[] { new StringKey("val", "NEW").Translate() });

            AddItem(new SelectionItemTraits(new StringKey("val", "NEW_X", new StringKey("val", type.ToUpper())).Translate(), "{NEW:" + type + "}", traits));
        }

        public void SelectTrait(string type, string trait)
        {
            foreach (TraitGroup tg in traitData)
            {
                if (tg.GetName().Equals(type))
                {
                    SelectTrait(tg, trait)
                    return;
                }
            }
        }

        public void ExcludeTrait(string type, string trait)
        {
            foreach (TraitGroup tg in traitData)
            {
                if (tg.GetName().Equals(type))
                {
                    ExcludeTrait(tg, trait)
                    return;
                }
            }
        }

        public void ExcludeTraitsWithExceptions(string type, IEnumerable<string> exceptions)
        {
            TraitGroup tg = null;
            foreach (TraitGroup group in traitData)
            {
                if (tg.GetName().Equals(type))
                {
                    tg = group;
                    break;
                }
            }

            if (tg == null) return;

            foreach (var t in tg.traits)
            {
                foreach (string e in exceptions)
                {
                    if (!t.Key.Equals(e))
                    {
                        ExcludeTrait(tg, trait)
                    }
                }
            }
        }

        public void ExcludeExpansions()
        {
            List<string> enabled = new List<string();
            enabled.Add(new StringKey("val", "base").Translate());
            foreach (string s in Game.Get().quest.qd.quest.packs)
            {
                enabled.Add(new StringKey("val", s).Translate());
            }
            ExcludeTraitsWithExceptions(new StringKey("val", "SOURCE").Translate(), enabled);
        }

        protected class SelectionItemTraits : SelectionItem
        {
            Dictionary<string, IEnumerable<string>> _traits = new Dictionary<string, IEnumerable<string>>();

            public SelectionItemTraits(string display, string key) : base(display, key)
            {
            }

            public SelectionItemTraits(string display, string key, Dictionary<string, IEnumerable<string>> traits) : base(display, key)
            {
                _traits = traits;
            }

            public SelectionItemTraits(SelectionItem item) : base(item.GetDisplay(), item.GetKey())
            {
            }

            public Dictionary<string, IEnumerable<string>> GetTraits()
            {
                return _traits;
            }
        }

        protected class TraitGroup
        {
            public Dictionary<string, Trait> traits = new Dictionary<string, Trait>();
            public List<SelectionItem> ungrouped = new List<SelectionItem>();
            public string _name = "";

            public TraitGroup(string name)
            {
                _name = name;
            }

            public string GetName()
            {
                return _name;
            }

            public bool NoneSelected()
            {
                bool anySelected = false;
                foreach (Trait t in traits.Values)
                {
                    anySelected |= t.selected;
                }
                return !anySelected;
            }

            public bool ActiveItem(SelectionItemTraits item)
            {
                if (NoneSelected()) return true;

                foreach (Trait t in traits.Values)
                {
                    if (t.items.Contains(item))
                    {
                        if (t.excluded)
                        {
                            // item contains excluded trait
                            return false;
                        }
                    }
                    else
                    {
                        if (t.selected)
                        {
                            // item does not contain selected trait
                            return false;
                        }
                    }
                }
                return true;
            }

            public void AddTraits(SelectionItemTraits item)
            {
                foreach (string trait in item.GetTraits()[_name])
                {
                    if (!traits.ContainsKey(trait))
                    {
                        traits.Add(trait, new Trait());
                    }
                }
            }

            public void AddItem(SelectionItemTraits item)
            {
                if (!item.GetTraits().ContainsKey(_name))
                {
                    ungrouped.Add(item);
                }
                else
                {
                    foreach (string s in item.GetTraits()[_name])
                    {
                        traits[s].items.Add(item);
                    }
                }
            }

            public class Trait
            {
                public bool selected = false;
                public bool excluded = false;
                public List<SelectionItem> items = new List<SelectionItem>();
            }
        }
    }
}
