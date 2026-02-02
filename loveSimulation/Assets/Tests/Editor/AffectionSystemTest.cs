using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using LoveSimulation.Core;
using LoveSimulation.Data;
using LoveSimulation.Events;

namespace LoveSimulation.Tests
{
    public class AffectionSystemTest
    {
        private bool _affectionChangedFired;
        private AffectionChanged _lastAffectionChanged;
        private bool _levelChangedFired;
        private AffectionLevelChanged _lastLevelChanged;

        [SetUp]
        public void SetUp()
        {
            GameData.Reset();
            CharacterDatabase.Reset();
            EventBus.Clear<AffectionChanged>();
            EventBus.Clear<AffectionLevelChanged>();

            _affectionChangedFired = false;
            _levelChangedFired = false;

            EventBus.Subscribe<AffectionChanged>(OnAffectionChanged);
            EventBus.Subscribe<AffectionLevelChanged>(OnAffectionLevelChanged);
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Unsubscribe<AffectionChanged>(OnAffectionChanged);
            EventBus.Unsubscribe<AffectionLevelChanged>(OnAffectionLevelChanged);
        }

        private void OnAffectionChanged(AffectionChanged evt)
        {
            _affectionChangedFired = true;
            _lastAffectionChanged = evt;
        }

        private void OnAffectionLevelChanged(AffectionLevelChanged evt)
        {
            _levelChangedFired = true;
            _lastLevelChanged = evt;
        }

        // --- 클램핑 테스트 ---

        [Test]
        public void AddAffection_최대값_클램핑()
        {
            GameData.SetMaxAffection("유나", 100);
            GameData.AddAffection("유나", 150);
            Assert.AreEqual(100, GameData.GetAffection("유나"));
        }

        [Test]
        public void AddAffection_최소값_클램핑()
        {
            GameData.AddAffection("유나", 10);
            GameData.AddAffection("유나", -50);
            Assert.AreEqual(0, GameData.GetAffection("유나"));
        }

        [Test]
        public void AddAffection_기본_최대값_100()
        {
            GameData.AddAffection("유나", 200);
            Assert.AreEqual(100, GameData.GetAffection("유나"));
        }

        [Test]
        public void SetMaxAffection_커스텀_최대값()
        {
            GameData.SetMaxAffection("유나", 50);
            GameData.AddAffection("유나", 100);
            Assert.AreEqual(50, GameData.GetAffection("유나"));
        }

        [Test]
        public void GetMaxAffection_미등록_캐릭터_기본값_100()
        {
            Assert.AreEqual(100, GameData.GetMaxAffection("미등록"));
        }

        // --- SetAffection 테스트 ---

        [Test]
        public void SetAffection_절대값_설정()
        {
            GameData.SetAffection("유나", 50);
            Assert.AreEqual(50, GameData.GetAffection("유나"));
        }

        [Test]
        public void SetAffection_클램핑_적용()
        {
            GameData.SetMaxAffection("유나", 80);
            GameData.SetAffection("유나", 999);
            Assert.AreEqual(80, GameData.GetAffection("유나"));
        }

        [Test]
        public void SetAffection_음수_0으로_클램핑()
        {
            GameData.SetAffection("유나", -10);
            Assert.AreEqual(0, GameData.GetAffection("유나"));
        }

        // --- AffectionChanged 이벤트 테스트 ---

        [Test]
        public void AddAffection_이벤트_발행()
        {
            GameData.AddAffection("유나", 5);

            Assert.IsTrue(_affectionChangedFired);
            Assert.AreEqual("유나", _lastAffectionChanged.CharacterId);
            Assert.AreEqual(0, _lastAffectionChanged.PreviousValue);
            Assert.AreEqual(5, _lastAffectionChanged.NewValue);
            Assert.AreEqual(5, _lastAffectionChanged.Delta);
        }

        [Test]
        public void AddAffection_클램핑시_실제_델타_반영()
        {
            GameData.SetMaxAffection("유나", 10);
            GameData.AddAffection("유나", 8);

            // 리셋 후 재측정
            _affectionChangedFired = false;
            GameData.AddAffection("유나", 5);

            Assert.IsTrue(_affectionChangedFired);
            Assert.AreEqual(8, _lastAffectionChanged.PreviousValue);
            Assert.AreEqual(10, _lastAffectionChanged.NewValue);
            Assert.AreEqual(2, _lastAffectionChanged.Delta);
        }

        // --- AffectionLevelChanged 이벤트 테스트 ---

        [Test]
        public void AddAffection_레벨변경_이벤트_CharacterData_없으면_미발행()
        {
            // CharacterDatabase에 등록된 캐릭터가 없으면 레벨 변경 이벤트 미발행
            GameData.AddAffection("미등록", 50);
            Assert.IsFalse(_levelChangedFired);
        }

        // --- CharacterData 레벨 계산 테스트 ---

        [Test]
        public void CharacterData_GetLevelName_임계값_검증()
        {
            var characterData = ScriptableObject.CreateInstance<CharacterData>();
            var so = new UnityEditor.SerializedObject(characterData);
            so.FindProperty("_characterId").stringValue = "테스트";

            var levels = so.FindProperty("_affectionLevels");
            levels.ClearArray();

            AddLevel(levels, "낯선 사이", 0);
            AddLevel(levels, "지인", 20);
            AddLevel(levels, "친구", 40);
            AddLevel(levels, "호감", 60);
            AddLevel(levels, "연인", 80);
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual("낯선 사이", characterData.GetLevelName(0));
            Assert.AreEqual("낯선 사이", characterData.GetLevelName(19));
            Assert.AreEqual("지인", characterData.GetLevelName(20));
            Assert.AreEqual("친구", characterData.GetLevelName(40));
            Assert.AreEqual("호감", characterData.GetLevelName(60));
            Assert.AreEqual("연인", characterData.GetLevelName(80));
            Assert.AreEqual("연인", characterData.GetLevelName(100));

            Object.DestroyImmediate(characterData);
        }

        // --- 기존 테스트 호환성 ---

        [Test]
        public void AddAffection_호감도_증가()
        {
            GameData.AddAffection("유나", 5);
            Assert.AreEqual(5, GameData.GetAffection("유나"));
        }

        [Test]
        public void AddAffection_호감도_감소()
        {
            GameData.AddAffection("유나", 10);
            GameData.AddAffection("유나", -3);
            Assert.AreEqual(7, GameData.GetAffection("유나"));
        }

        [Test]
        public void Reset_maxAffection도_초기화()
        {
            GameData.SetMaxAffection("유나", 50);
            GameData.Reset();
            Assert.AreEqual(100, GameData.GetMaxAffection("유나"));
        }

        private static void AddLevel(UnityEditor.SerializedProperty array, string name, int threshold)
        {
            int index = array.arraySize;
            array.InsertArrayElementAtIndex(index);
            var element = array.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("LevelName").stringValue = name;
            element.FindPropertyRelative("Threshold").intValue = threshold;
        }
    }
}
