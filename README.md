# Unity AudioManager System

Unity向けの高機能なオーディオ管理システムです。Addressables対応、オブジェクトプール、自動パス生成など、プロフェッショナルなゲーム開発に必要な機能を提供します。

## 必須ライブラリ
- Addressables

## 🎵 主な機能

- **SE (Sound Effect) 管理**: 効果音の再生・停止・音量制御
- **BGM (Background Music) 管理**: 背景音楽の再生・停止・音量制御・フェード機能
- **Addressables対応**: 非同期読み込みとメモリ効率的な管理
- **オブジェクトプール**: AudioSourceの効率的な再利用
- **自動パス生成**: 型安全なオーディオファイルパス管理
- **設定管理**: 音量設定とプール数の調整

## 🚀 セットアップ

### 1. 基本設定

AudioManagerは自動的に初期化されます。特別な設定は不要です。

```csharp
// ゲーム開始時に自動で初期化される
// SEManager.Instance と BGMManager.Instance が利用可能
```

### 2. Addressablesの設定

オーディオファイルをAddressablesに登録してください：

1. **Addressables Groups**を作成
   - `AudioManagerSE` グループ（SE用）
   - `AudioManagerBGM` グループ（BGM用）
  
2. **Addressables Labels**を作成
  - `BGM`ラベル
  - `SE`ラベル

4. **オーディオファイルを追加**
   - mp3, wav, ogg ファイルを各グループに追加
   - アドレス名を設定（例: "ButtonClick", "TitleTheme"）
   - BGMには`BGM`ラベル, SEには`SE`ベルを付与

### 3. パスの自動生成

メニューから実行してパスクラスを生成：

```
Tools > AudioManager > Update All Audio Paths
```

これにより以下のクラスが自動生成されます：
- `SEPath.cs`
- `BGMPath.cs`

## 🎮 使い方

### SE（効果音）の再生

```csharp
using Yuffter.AudioManager.SE;

// SE再生
SEManager.Instance.Play("ButtonClick");

// または生成されたパスクラスを使用
SEManager.Instance.Play(SEPath.ButtonClick);

// SE停止
SEManager.Instance.Stop("ButtonClick");

// 全SE停止
SEManager.Instance.StopAll();
```

### BGM（背景音楽）の再生

```csharp
using Yuffter.AudioManager.BGM;

// BGM再生
BGMManager.Instance.Play("TitleTheme");

// または生成されたパスクラスを使用
BGMManager.Instance.Play(BGMPath.TitleTheme);

// BGM停止
BGMManager.Instance.StopAll();

// 一時停止・再開
BGMManager.Instance.PauseAll();
BGMManager.Instance.ResumeAll();
```

### 音量制御

```csharp
// 音量設定（0.0f〜1.0f）
SEManager.Instance.SetVolume(0.8f);
BGMManager.Instance.SetVolume(0.6f);

// 音量設定の保存・読み込み
AudioSettings.Instance.SetSEVolume(0.8f);
AudioSettings.Instance.SetBGMVolume(0.6f);
```

## 🎛️ 設定

### AudioSettings

音量設定やプール数は `AudioSettings` で管理されます：

```csharp
// プール数設定
AudioSettings.Instance.SEAudioSourcePoolSize = 10;
AudioSettings.Instance.BGMAudioSourcePoolSize = 5;

// 音量設定（PlayerPrefsに自動保存）
AudioSettings.Instance.SetMasterVolume(1.0f);
AudioSettings.Instance.SetSEVolume(0.8f);
AudioSettings.Instance.SetBGMVolume(0.6f);
```

## 📁 ファイル構成

```
AudioManager/
├── Core/
│   ├── AudioPlayer.cs          # オーディオプレイヤー
│   ├── AudioManagerBase.cs     # 基底クラス
│   ├── IAudioManager.cs        # インターフェース
│   ├── SingletonMonoBehaviour.cs # シングルトン基底
│   └── AudioPathGenerator.cs   # パス自動生成
├── SE/
│   ├── SEManager.cs            # SE管理
│   └── SEPath.cs               # SE パス（自動生成）
├── BGM/
│   ├── BGMManager.cs           # BGM管理
│   └── BGMPath.cs              # BGM パス（自動生成）
├── Settings/
│   └── AudioSettings.cs       # 設定管理
└── Test/
    └── AudioPlayTest.cs        # テストスクリプト
```

## 🔧 高度な使用例

### カスタムAudioPlayer

```csharp
using Yuffter.AudioManager.Core;

// 独自のAudioPlayerを作成
AudioSource audioSource = GetComponent<AudioSource>();
AudioPlayer player = new AudioPlayer(audioSource);

// 詳細パラメータで再生
player.Play(audioClip, volume: 0.8f, pitch: 1.2f, loop: true);
```

### 複数BGMの管理

```csharp
// BGMの切り替え
BGMManager.Instance.StopAll();
BGMManager.Instance.Play(BGMPath.BattleTheme);

// フェード機能（今後実装予定）
// BGMManager.Instance.FadeOut(1.0f);
// BGMManager.Instance.FadeIn(BGMPath.BattleTheme, 1.0f);
```

## 🎯 ベストプラクティス

### 1. パフォーマンス最適化

```csharp
// よく使用されるSEは事前に読み込み
SEManager.Instance.Play(SEPath.ButtonClick);

// 不要になったら解放
SEManager.Instance.Release();
BGMManager.Instance.Release();
```

### 2. エラーハンドリング

```csharp
// 存在しないパスを指定した場合、警告ログが出力される
SEManager.Instance.Play("NonExistentSound");
// Warning: Failed to load AudioClip at path: NonExistentSound
```

### 3. メモリ管理

```csharp
// シーン切り替え時に不要なオーディオを解放
private void OnDestroy()
{
    SEManager.Instance.Release();
    BGMManager.Instance.Release();
}
```

## 🐛 トラブルシューティング

### よくある問題と解決方法

1. **音が再生されない**
   - Addressablesグループにオーディオファイルが登録されているか確認
   - パスの名前が正しいか確認
   - オーディオファイルの形式がサポートされているか確認（mp3, wav, ogg）

2. **パスクラスが生成されない**
   - `Tools > AudioManager > Update All Audio Paths` を実行
   - Addressablesのグループ名が正しいか確認（`AudioManagerSE`, `AudioManagerBGM`）

3. **音量が変更されない**
   - `AudioSettings.Instance.SetXXXVolume()` を使用
   - マスター音量が0になっていないか確認

## 📝 今後の拡張予定

- クロスフェード機能
- 3D音響対応
- 音声圧縮設定の最適化
- リアルタイム音量調整UI

## 🤝 貢献

このプロジェクトへの貢献を歓迎します。Issues や Pull Requests をお気軽にお送りください。
