using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using LoveSimulation.Events;

namespace LoveSimulation.Core
{
    /// <summary>
    /// JSON 기반 저장/로드 매니저. 5개 슬롯 지원.
    /// </summary>
    public class SaveLoadManager : Singleton<SaveLoadManager>
    {
        private const int MaxSlots = 5;
        private const string SaveFolder = "SaveData";
        private const string FilePrefix = "save_";
        private const string FileExtension = ".json";

        private string _saveFolderPath;

        protected override void OnSingletonAwake()
        {
            _saveFolderPath = Path.Combine(Application.persistentDataPath, SaveFolder);
            EnsureSaveFolder();
            Debug.Log($"[SaveLoadManager] 초기화 완료. 저장 경로: {_saveFolderPath}");
        }

        private void OnEnable()
        {
            EventBus.Subscribe<SaveRequested>(OnSaveRequested);
            EventBus.Subscribe<LoadRequested>(OnLoadRequested);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<SaveRequested>(OnSaveRequested);
            EventBus.Unsubscribe<LoadRequested>(OnLoadRequested);
        }

        private void OnSaveRequested(SaveRequested e)
        {
            Save(e.SlotIndex);
        }

        private void OnLoadRequested(LoadRequested e)
        {
            Load(e.SlotIndex);
        }

        /// <summary>
        /// 지정 슬롯에 현재 게임 데이터를 저장.
        /// </summary>
        public void Save(int slotIndex)
        {
            if (!IsValidSlot(slotIndex))
            {
                return;
            }

            GameManager gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogError("[SaveLoadManager] GameManager 인스턴스를 찾을 수 없음.");
                PublishSaveCompleted(slotIndex, false);
                return;
            }

            var data = new SaveData
            {
                SceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                GameState = gameManager.CurrentState,
                PlayTime = gameManager.PlayTime,
                SaveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            bool success = WriteToFile(slotIndex, data);
            PublishSaveCompleted(slotIndex, success);
        }

        /// <summary>
        /// 지정 슬롯에서 게임 데이터를 로드.
        /// </summary>
        public void Load(int slotIndex)
        {
            if (!IsValidSlot(slotIndex))
            {
                return;
            }

            SaveData data = ReadFromFile(slotIndex);
            if (data == null)
            {
                PublishLoadCompleted(slotIndex, false);
                return;
            }

            ApplyLoadedData(data);
            PublishLoadCompleted(slotIndex, true);
        }

        /// <summary>
        /// 지정 슬롯의 메타 정보를 반환.
        /// </summary>
        public SaveSlotInfo GetSlotInfo(int slotIndex)
        {
            if (!IsValidSlot(slotIndex))
            {
                return CreateEmptySlotInfo(slotIndex);
            }

            SaveData data = ReadFromFile(slotIndex);
            if (data == null)
            {
                return CreateEmptySlotInfo(slotIndex);
            }

            return new SaveSlotInfo
            {
                SlotIndex = slotIndex,
                SaveDate = data.SaveDate,
                SceneName = data.SceneName,
                PlayTime = data.PlayTime,
                IsEmpty = false
            };
        }

        /// <summary>
        /// 모든 슬롯의 메타 정보를 반환.
        /// </summary>
        public SaveSlotInfo[] GetAllSlotInfos()
        {
            var infos = new SaveSlotInfo[MaxSlots];
            for (int i = 0; i < MaxSlots; i++)
            {
                infos[i] = GetSlotInfo(i);
            }
            return infos;
        }

        /// <summary>
        /// 지정 슬롯의 세이브 파일을 삭제.
        /// </summary>
        public bool DeleteSlot(int slotIndex)
        {
            if (!IsValidSlot(slotIndex))
            {
                return false;
            }

            string path = GetFilePath(slotIndex);
            if (!File.Exists(path))
            {
                return true;
            }

            try
            {
                File.Delete(path);
                Debug.Log($"[SaveLoadManager] 슬롯 {slotIndex} 삭제 완료.");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveLoadManager] 슬롯 {slotIndex} 삭제 실패: {e.Message}");
                return false;
            }
        }

        private bool WriteToFile(int slotIndex, SaveData data)
        {
            try
            {
                EnsureSaveFolder();
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                string path = GetFilePath(slotIndex);
                File.WriteAllText(path, json);
                Debug.Log($"[SaveLoadManager] 슬롯 {slotIndex} 저장 완료.");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveLoadManager] 슬롯 {slotIndex} 저장 실패: {e.Message}");
                return false;
            }
        }

        private SaveData ReadFromFile(int slotIndex)
        {
            string path = GetFilePath(slotIndex);
            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                string json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<SaveData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveLoadManager] 슬롯 {slotIndex} 로드 실패: {e.Message}");
                return null;
            }
        }

        private void ApplyLoadedData(SaveData data)
        {
            GameManager gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogError("[SaveLoadManager] GameManager 인스턴스를 찾을 수 없음.");
                return;
            }

            gameManager.SetPlayTime(data.PlayTime);
            gameManager.ChangeState(data.GameState);

            // 씬 전환 요청
            EventBus.Publish(new SceneTransitionRequested
            {
                SceneName = data.SceneName,
                FadeDuration = 0.5f
            });
        }

        private void PublishSaveCompleted(int slotIndex, bool success)
        {
            EventBus.Publish(new SaveCompleted
            {
                SlotIndex = slotIndex,
                Success = success
            });
        }

        private void PublishLoadCompleted(int slotIndex, bool success)
        {
            EventBus.Publish(new LoadCompleted
            {
                SlotIndex = slotIndex,
                Success = success
            });
        }

        private bool IsValidSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MaxSlots)
            {
                Debug.LogError($"[SaveLoadManager] 잘못된 슬롯 인덱스: {slotIndex} (유효 범위: 0~{MaxSlots - 1})");
                return false;
            }
            return true;
        }

        private string GetFilePath(int slotIndex)
        {
            return Path.Combine(_saveFolderPath, $"{FilePrefix}{slotIndex}{FileExtension}");
        }

        private void EnsureSaveFolder()
        {
            if (!Directory.Exists(_saveFolderPath))
            {
                Directory.CreateDirectory(_saveFolderPath);
            }
        }

        private SaveSlotInfo CreateEmptySlotInfo(int slotIndex)
        {
            return new SaveSlotInfo
            {
                SlotIndex = slotIndex,
                IsEmpty = true
            };
        }
    }
}
