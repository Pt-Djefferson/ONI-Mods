﻿using Harmony;
using STRINGS;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BetterInfoCards
{
    public class StatusDataManager
    {
        public const string title = "Title";
        public const string germs = "Germs";
        public const string temp = "Temp";

        private const string sumSuffix = " (Σ)";
        private const string avgSuffix = " (μ)";

        private static readonly string oreMass = Db.Get().MiscStatusItems.OreMass.Name;
        private static readonly string oreTemp = Db.Get().MiscStatusItems.OreTemp.Name;

        private static readonly Status<float> titleStatus = new Status<float>(
                title,
                data => {
                    GameObject go = data as GameObject;
                    KPrefabID prefabID = go.GetComponent<KPrefabID>();
                    if (prefabID != null && Assets.IsTagCountable(prefabID.PrefabTag))
                        return go.GetComponent<PrimaryElement>().Units;
                    return 1;
                },
                (template, counts) => template + " x " + counts.Sum(),
                infoCards => new List<List<InfoCard>>() { infoCards });

        private static readonly Status<DiseasePair> germStatus = new Status<DiseasePair>(
                germs,
                data => {
                    PrimaryElement element = ((GameObject)data).GetComponent<PrimaryElement>();
                    return new DiseasePair(element.DiseaseIdx, element.DiseaseCount);
                },
                // Impossible for multiple storages to overlap, so no need to worry about that part of the germ text since it will never be overwritten
                (template, pairs) => {
                    string text = UI.OVERLAYS.DISEASE.NO_DISEASE;
                    if (pairs[0].diseaseIdx != 255)
                        text = GameUtil.GetFormattedDisease(pairs[0].diseaseIdx, pairs.Sum(x => x.diseaseCount), true) + sumSuffix;
                    return text;
                },
                infoCards => {
                    List<DiseasePair> pairs = infoCards.Select(x => (DiseasePair)x.textValues[germs]).ToList();
                    var splits = GetSplitLists(infoCards, pairs.Select(x => (float)x.diseaseIdx).ToList(), 1f);
                    return splits;
                });

        private static readonly Status<float> tempStatus = new Status<float>(
                oreTemp,
                data => ((GameObject)data).GetComponent<PrimaryElement>().Temperature,
                (template, temps) => template.Replace("{Temp}", GameUtil.GetFormattedTemperature(temps.Average())) + avgSuffix,
                infoCards => GetSplitLists(infoCards, infoCards.Select(x => (float)x.textValues[oreTemp]).ToList(), 10f));

        private static readonly Status<float> massStatus = new Status<float>(
                oreMass,
                data => ((GameObject)data).GetComponent<PrimaryElement>().Mass,
                (template, masses) => template.Replace("{Mass}", GameUtil.GetFormattedMass(masses.Sum())) + sumSuffix,
                infoCards => new List<List<InfoCard>>() { infoCards });

        public static readonly Dictionary<string, ITextDataConverter> statusConverter = new Dictionary<string, ITextDataConverter>()
        {
            { title, titleStatus },
            { germs, germStatus },
            { temp, tempStatus },
            { oreMass, massStatus },
            { oreTemp, tempStatus },
        };

        private static List<List<InfoCard>> GetSplitLists(List<InfoCard> cards, List<float> values, float maxRange)
        {
            cards = cards.OrderBy(x => values[cards.IndexOf(x)]).ToList();
            values.Sort();

            var splits = new List<List<InfoCard>>();

            float range = values[values.Count - 1] - values[0];
            int maxLists = Mathf.CeilToInt(range / maxRange);

            if (maxLists <= 1)
            {
                splits.Add(cards);
                return splits;
            }

            var listStartIndices = new List<int>() { 0 };
            float listStartValue = values[0];

            for (int i = 0; i < values.Count; i++)
            {
                float value = values[i];

                if (value > listStartValue + maxRange)
                {
                    listStartIndices.Add(i);
                    listStartValue = value;
                }   
            }
            listStartIndices.Add(values.Count);

            for (int i = 0; i < listStartIndices.Count - 1; i++)
            {
                int startIndex = listStartIndices[i];
                int endIndex = listStartIndices[i + 1];
                splits.Add(cards.GetRange(startIndex, endIndex - startIndex));
            }

            return splits;
        }

        private static string RemoveQuantityCount(string text)
        {
            var charStack = new Stack<char>();
            int i;
            for (i = text.Length - 1; i >= 0; i--)
            {
                if (!char.IsDigit(text[i]))
                    break;

                charStack.Push(text[i]);
            }

            text = text.Remove(i - 1, text.Length - i + 1);

            return text;
        }

        public interface ITextDataConverter
        {
            string GetTextOverride(string original, List<object> values);
            List<List<InfoCard>> GetSplitLists(List<InfoCard> cards);
            object GetTextValue(object data);
        }

        public class Status<T> : ITextDataConverter
        {
            public Status(string name, Func<object, T> getValue, Func<string, List<T>, string> getTextOverride, Func<List<InfoCard>, List<List<InfoCard>>> getSplitLists)
            {
                this.name = name;
                this.getStatusValue = getValue;
                this.getTextOverride = getTextOverride;
                this.getSplitLists = getSplitLists;
                //values = infoCards.ForEach(x => getStatusValue(x.statusDatas[name]));
            }

            private readonly string name;
            private readonly Func<object, T> getStatusValue;
            private readonly Func<string, List<T>, string> getTextOverride;
            private readonly Func<List<InfoCard>, List<List<InfoCard>>> getSplitLists;

            public object GetTextValue(object data)
            {
                return getStatusValue(data);
            }

            public string GetTextOverride(string original, List<object> values)
            {
                return getTextOverride(original, values.Cast<T>().ToList());
            }

            public List<List<InfoCard>> GetSplitLists(List<InfoCard> cards)
            {
                return getSplitLists(cards);
            }
        }

        private struct DiseasePair
        {
            public byte diseaseIdx;
            public int diseaseCount;

            public DiseasePair(byte diseaseIdx, int diseaseCount)
            {
                this.diseaseIdx = diseaseIdx;
                this.diseaseCount = diseaseCount;
            }
        }
    }
}
