# Unity AudioManager

Addressables ベースの Unity 向けオーディオ管理ライブラリです。SE / BGM の再生を、型安全な enum・オブジェクトプール・非同期ロード・ミキサー連動の音量制御でまとめて扱えます。

- **Unity 6000.0.41f1 (Unity 6)** / Universal Render Pipeline
- 依存: `com.unity.addressables`
- 名前空間: `Yuffter.AudioManager`

## 特徴

- **型安全な ID** — `AudioLibrary`(ScriptableObject) に登録した音源から `Se` / `Bgm` enum を自動生成。文字列パス指定は不要。
- **非同期ロード** — クリップは `AssetReferenceT<AudioClip>` 経由で Addressables から非同期ロードし、内部でキャッシュ。
- **オブジェクトプール** — `AudioSource` を使い回し、再生ごとの生成コストを排除。
- **世代管理ハンドル** — 再生中の 1 音を指す軽量な `AudioHandle` で、停止・フェードアウト・音量変更を安全に操作。
- **ミキサー連動の音量制御** — Master / SE / BGM をリニア値(0〜1)で設定でき、内部で dB に変換して `AudioMixer` へ反映。設定は `PlayerPrefs` に自動保存。
- **BGM のシングルボイス管理** — BGM の切り替え時に旧曲を自動でフェードアウトし、クリップを解放。

## インストール

[Releases](../../releases) から最新の `AudioManager-vX.Y.Z.unitypackage` をダウンロードし、Unity プロジェクトにインポートします。

1. `.unitypackage` をダウンロード
2. Unity で `Assets > Import Package > Custom Package...` から取り込む(またはファイルをプロジェクトにドラッグ&ドロップ)
3. `AudioManager` フォルダ一式(スクリプト・`AudioMixer` ・プレハブ)がプロジェクトに追加される

> 事前に Package Manager で **Addressables** を導入しておいてください。

各バージョンのパッケージは、`v*` タグの push をトリガーに GitHub Actions(`.github/workflows/release-unitypackage.yml`)で自動ビルドされ、Release に添付されます。

## セットアップ

### 1. 音源を Addressables に登録

再生したい `AudioClip` を Addressables グループに追加します(SE 用 `AudioManagerSE`、BGM 用 `AudioManagerBGM` を想定)。

### 2. AudioLibrary を作成して音源を登録

`Create > Yuffter/AudioManager/Audio Library` で `AudioLibrary` アセットを作成し、`Entries` に音源を追加します。各エントリの項目:

| フィールド | 説明 |
|-----------|------|
| `Key` | enum 名になる識別子(例: `ButtonClick`) |
| `Category` | `Se` または `Bgm` |
| `Clip` | `AssetReferenceT<AudioClip>`(Addressables 参照) |
| `Volume` / `Pitch` | 既定の音量・ピッチ |
| `Loop` | 既定でループするか |
| `SpatialBlend` | 0 = 2D / 1 = 3D |

`Id` はエントリ追加時に自動で採番されます(`OnValidate`)。**並び替えても ID は変わらない**ため、参照が壊れません。

### 3. enum を生成

`AudioLibrary` を保存すると `Assets/AudioManager/Generated/AudioIds.cs` が**自動再生成**されます。手動で再生成する場合はメニューから:

```
Tools > AudioManager > Generate Enums
```

これで `enum Se` / `enum Bgm` が生成され、`Se.ButtonClick` のように参照できます。

> `AudioIds.cs` は自動生成ファイルです。直接編集せず、`AudioLibrary` を編集してください。

### 4. シーンに AudioManager を配置

`AudioManager` コンポーネントをシーンに 1 つ配置し、インスペクタで次を割り当てます:

- `Library` … 作成した `AudioLibrary`
- `Mixer` … `AudioMixer`(`MasterVol` / `SeVol` / `BgmVol` を公開パラメータとして設定しておく)
- `Se Group` / `Bgm Group` … ミキサーの各 `AudioMixerGroup`
- `Prewarm` … プールの事前生成数(既定 8)

`AudioManager` は `Awake` で `DontDestroyOnLoad` になり、`AudioManager.Instance` からアクセスできます。プレハブ `Assets/AudioManager/AudioManager.prefab` と `AudioMixer.mixer` を利用すると設定済みの状態から始められます。

## 使い方

### SE の再生

```csharp
using Yuffter.AudioManager;

// クリップのロード完了を待つ場合（await はロード完了時点で戻る。再生の終了は待たない）
await AudioManager.Instance.PlaySe(Se.ButtonClick);

// 撃ちっぱなし(await しない)
AudioManager.Instance.PlaySe(Se.ButtonClick);
```

### BGM の再生

```csharp
// 既定でループ再生。同じ BGM が再生中なら何もしない
await AudioManager.Instance.PlayBgm(Bgm.TitleTheme);

// 別の BGM に切り替えると、旧曲は自動でフェードアウト＆解放される
await AudioManager.Instance.PlayBgm(Bgm.BattleTheme);

// 停止(フェードアウト秒数を指定可能)
AudioManager.Instance.StopBgm(fade: 1f);

// 再生中か確認
bool playing = AudioManager.Instance.IsBgmPlaying(Bgm.TitleTheme);
```

### 再生パラメータ (`PlayParams`)

`PlayParams` で 1 回の再生を細かく制御できます。未指定の項目はエントリの既定値が使われます。

```csharp
var p = PlayParams.Default;
p.Volume = 0.8f;
p.Pitch = 1.2f;
p.FadeIn = 0.5f;          // フェードイン秒数
p.Delay = 0.1f;           // 再生開始の遅延
p.Loop = true;            // null ならエントリの既定
p.SpatialBlend = 1f;      // -1 ならエントリの既定(0=2D, 1=3D)
p.Position = worldPos;    // 3D 音源の位置
p.Follow = targetTransform; // 指定すると位置を追従

await AudioManager.Instance.PlaySe(Se.Explosion, p);
```

### 再生中の音の操作 (`AudioHandle`)

`PlaySe` / `PlayBgm` は再生中の 1 音を指す `AudioHandle` を返します。プールでソースが再利用されると、そのハンドルは自動的に無効(no-op)になります。

```csharp
var handle = await AudioManager.Instance.PlaySe(Se.Loop);

if (handle.IsPlaying) { /* ... */ }
handle.SetVolume(0.5f);
handle.FadeOut(1f);   // フェードアウトして停止・解放
handle.Stop();        // 即時停止
```

### 音量制御

Master / SE / BGM をリニア値(0〜1)で設定します。内部で dB に変換して `AudioMixer` に反映され、`PlayerPrefs` に自動保存・次回起動時に自動復元されます。

```csharp
AudioManager.Instance.SetMasterVolume(1.0f);
AudioManager.Instance.SetSeVolume(0.8f);
AudioManager.Instance.SetBgmVolume(0.6f);

// 現在値の参照
float m = AudioManager.Instance.Volume.Master;
```

> リニア 0〜1 → dB 変換は `20 * log10(value)` を使用し、0 付近は −80dB(無音)にクランプしています。

### ロード制御

```csharp
// よく使う SE を事前ロードして初回再生の遅延をなくす
await AudioManager.Instance.Preload(Se.ButtonClick, Se.Cancel);

// SE のクリップキャッシュをまとめて解放(SE は再生後もキャッシュ保持されるため)
AudioManager.Instance.UnloadSeBank();
```

## アーキテクチャ

| クラス | 役割 |
|--------|------|
| `AudioManager` | 中心となる `MonoBehaviour`。再生 API・Addressables ロード/キャッシュ・BGM の状態管理。 |
| `AudioLibrary` (SO) | 音源定義 `AudioEntry` のリスト。安定 ID を採番。 |
| `AudioEnumGenerator` (Editor) | ライブラリから `Se` / `Bgm` enum を生成。保存時に自動実行。 |
| `AudioPool` | `AudioSourceController` のスタックベースのプール。 |
| `AudioSourceController` | プールされた `AudioSource` 1 個を管理。フェード・追従・自動解放。 |
| `AudioHandle` / `PlayParams` | 再生中の音への軽量ハンドルと再生パラメータ(いずれも struct)。 |
| `VolumeController` | リニア↔dB 変換と `AudioMixer` 反映、`PlayerPrefs` 保存。 |

### 設計上のポイント

- **再生 API が async なのは Addressables のロードが非同期だから。** クリップの実体はメモリに載っていないため、`PlaySe` / `PlayBgm` はロード完了後に再生を開始します。`await` が待つのは**クリップのロード完了まで**で、再生の終了は待ちません(戻り値は再生開始直後の `AudioHandle`)。2 回目以降はキャッシュから即返るため、待ちたくない場合は `await` を省けます。
- **BGM はシングルボイス。** `AudioManager` が現在の BGM を追跡し、切り替え時に旧曲をフェードアウトしてクリップを解放します。SE は多重再生されるポリフォニックなボイスです。
- **ハンドルは世代でガード。** `AudioSourceController` は再生・解放のたびに `Generation` を進め、`AudioHandle` は取得時の世代と一致するときだけ有効です。プールで再利用済みのソースへの操作は安全に無視されます。

## 注意

- 本ライブラリには CLI ビルドスクリプトはありません。Unity エディタ(6000.0.41f1)で開いて利用します。
- `AudioClip` を再生するには、必ず `AudioLibrary` のエントリと Addressables 参照を設定してください。

## ライセンス

[MIT License](LICENSE) の下で公開しています。
