using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using LoveSimulation.Core;

namespace LoveSimulation.Tests
{
    public class ChoiceSystemTest
    {
        [SetUp]
        public void SetUp()
        {
            GameData.Reset();
        }

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
        public void GetAffection_미등록_캐릭터_0_반환()
        {
            Assert.AreEqual(0, GameData.GetAffection("미등록"));
        }

        [Test]
        public void AddAffection_null_무시()
        {
            GameData.AddAffection(null, 5);
            Assert.AreEqual(0, GameData.GetAffection(null));
        }

        [Test]
        public void AddAffection_빈문자열_무시()
        {
            GameData.AddAffection("", 5);
            Assert.AreEqual(0, GameData.GetAffection(""));
        }

        [Test]
        public void SetFlag_플래그_설정_조회()
        {
            GameData.SetFlag("met_yuna");
            Assert.IsTrue(GameData.GetFlag("met_yuna"));
        }

        [Test]
        public void GetFlag_미설정_플래그_false_반환()
        {
            Assert.IsFalse(GameData.GetFlag("unknown_flag"));
        }

        [Test]
        public void SetFlag_null_무시()
        {
            GameData.SetFlag(null);
            Assert.IsFalse(GameData.GetFlag(null));
        }

        [Test]
        public void SetFlag_빈문자열_무시()
        {
            GameData.SetFlag("");
            Assert.IsFalse(GameData.GetFlag(""));
        }

        [Test]
        public void ExportTo_ImportFrom_왕복_호감도()
        {
            GameData.AddAffection("유나", 10);
            GameData.AddAffection("미사키", 3);

            var saveData = new SaveData();
            GameData.ExportTo(saveData);

            GameData.Reset();
            Assert.AreEqual(0, GameData.GetAffection("유나"));

            GameData.ImportFrom(saveData);
            Assert.AreEqual(10, GameData.GetAffection("유나"));
            Assert.AreEqual(3, GameData.GetAffection("미사키"));
        }

        [Test]
        public void ExportTo_ImportFrom_왕복_플래그()
        {
            GameData.SetFlag("met_yuna");
            GameData.SetFlag("ch01_done");

            var saveData = new SaveData();
            GameData.ExportTo(saveData);

            GameData.Reset();
            Assert.IsFalse(GameData.GetFlag("met_yuna"));

            GameData.ImportFrom(saveData);
            Assert.IsTrue(GameData.GetFlag("met_yuna"));
            Assert.IsTrue(GameData.GetFlag("ch01_done"));
        }

        [Test]
        public void Reset_모든_데이터_초기화()
        {
            GameData.AddAffection("유나", 5);
            GameData.SetFlag("test_flag");

            GameData.Reset();

            Assert.AreEqual(0, GameData.GetAffection("유나"));
            Assert.IsFalse(GameData.GetFlag("test_flag"));
        }

        [Test]
        public void ExportTo_null_SafeData_예외없음()
        {
            LogAssert.Expect(LogType.Error, "[GameData] null SaveData에 내보내기 시도.");
            Assert.DoesNotThrow(() => GameData.ExportTo(null));
        }

        [Test]
        public void ImportFrom_null_SaveData_예외없음()
        {
            LogAssert.Expect(LogType.Error, "[GameData] null SaveData에서 가져오기 시도.");
            Assert.DoesNotThrow(() => GameData.ImportFrom(null));
        }

        [Test]
        public void ImportFrom_빈_딕셔너리_예외없음()
        {
            var saveData = new SaveData
            {
                AffectionData = null,
                Flags = null
            };

            Assert.DoesNotThrow(() => GameData.ImportFrom(saveData));
            Assert.AreEqual(0, GameData.GetAffection("유나"));
        }
    }
}
