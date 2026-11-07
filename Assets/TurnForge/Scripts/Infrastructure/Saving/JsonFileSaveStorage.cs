using System;
using System.IO;
using System.Security;
using UnityEngine;



namespace TF.Infrastructure.Saving
{
    /// <summary>
    /// 第3回で実装する、ゲーム内容に依存しないJSON保存処理
    /// </summary>
    public sealed class JsonFileSaveStorage : ISaveStorage
    {
        /// <summary>
        /// 保存ファイルを配置するフォルダー
        /// </summary>
        private readonly string _directoryPath;

        /// <summary>
        /// 保存先フォルダーを受け取る
        /// </summary>
        public JsonFileSaveStorage(string directoryPath)
        {
            _directoryPath = directoryPath;
        }
        private string GetFilePath(string key)
        {
            return Path.Combine(_directoryPath, $"save_{key}.json");
        }
        private bool IsValiKey(string key)
        {
            foreach (char character in key)
            {
                bool isAllowed =
                    (character >= 'a' && character <= 'z')
                    || (character >= 'A' && character <= 'Z')
                    || (character >= '0' && character <= '9')
                    || character == '-'
                    || character == '_';
                if (!isAllowed)
                {
                    return false;
                }
            }
            return true;
        }
        private static bool IsStorageException(Exception exception)
        {
            return exception is IOException
                || exception is UnauthorizedAccessException
                || exception is SecurityException
                || exception is ArgumentException
                || exception is NotSupportedException;
        }
        private static void DelateTemporayFile(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            try
            {
                File.Delete(path);
            }
            catch (Exception exception) when (IsStorageException(exception))
            {
                Debug.LogWarning($"一時ファイルを削除できませんでした。:{exception.Message}");
            }
        }
        /// <summary>
        /// 指定したキーでデータを保存する。配布時は書き込まない。
        /// </summary>
        public bool TrySave<T>(string key, T data) where T : class
        {
            if (data == null || string.IsNullOrWhiteSpace(_directoryPath) || !IsValiKey(key))
            {
                return false;
            }
            //書き込み途中のファイルを保存データから分離する
            string tmpPath = null;
            try
            {
                string json = JsonUtility.ToJson(data, true);

                Directory.CreateDirectory(_directoryPath);

                string filePath = GetFilePath(key);

                tmpPath = $"{filePath}.{Guid.NewGuid().ToString("N")}.tmp";

                File.WriteAllText(tmpPath, json);

                if (File.Exists(filePath))
                {
                    File.Replace(tmpPath, filePath, null);
                }
                else
                {
                    File.Move(tmpPath, filePath);
                }
                return true;
            }
            catch (Exception exception) when (IsStorageException(exception))
            {
                Debug.LogWarning($"セーブに失敗しました。:{exception.Message}");
                return false;
            }
            finally
            {
                DelateTemporayFile(tmpPath);
            }
            return false;
        }

        /// <summary>
        /// 保存済みデータを読み込む。配布時は読み込まない。
        /// </summary>
        public bool TryLoad<T>(string key, out T data, out SaveLoadStatus status) where T : class
        {
            data = null;
            status = SaveLoadStatus.Failed;

            if (!IsValiKey(key) || string.IsNullOrWhiteSpace(_directoryPath))
            {
                return false;
            }
            try
            {
                string filePath = GetFilePath(key);
                Debug.Log($"保存パス={filePath}");
                string json = File.ReadAllText(filePath);
                if (string.IsNullOrEmpty(json))
                {
                    return false;
                }
                data = JsonUtility.FromJson<T>(json);
                if (data == null)
                {
                    return false;
                }
                status = SaveLoadStatus.Success;
                return true;
            }
            catch (DirectoryNotFoundException)
            {
                status = SaveLoadStatus.NotFound;
                return false;
            }
            catch (FileNotFoundException)
            {
                status = SaveLoadStatus.NotFound;
                return false;
            }
            catch (Exception exception) when (IsStorageException(exception))
            {
                Debug.LogWarning($"セーブの読み込みに失敗しました。key:{key} messege {exception.Message}");
                data = null;
                return false;
            }
            return false;
        }
    }
}