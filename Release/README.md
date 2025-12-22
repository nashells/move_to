# MoveTo - Context Menu File Mover

ファイルやフォルダを右クリックメニューから素早く移動できる Windows シェル拡張です。

## 動作環境

- Windows 10/11 (64-bit)
- .NET Framework 4.8

## インストール方法

1. **管理者権限で PowerShell を開く**
   - スタートメニューで「PowerShell」を検索
   - 右クリック → 「管理者として実行」

2. **インストールスクリプトを実行**
   ```powershell
   Set-Location <このフォルダのパス>
   powershell -ExecutionPolicy Bypass -File .\install.ps1
   ```

3. **設定ファイルを編集**
   - パス: `%LOCALAPPDATA%\MoveTo\config.json`
   - 移動先フォルダを追加してください

## 設定ファイルの書き方

```json
{
  "destinations": [
    { "displayName": "Temp", "path": "C:\\Temp" },
    { "displayName": "ダウンロード", "path": "C:\\Users\\YourName\\Downloads" },
    { "displayName": "作業フォルダ", "path": "D:\\Work" }
  ]
}
```

- `displayName`: メニューに表示される名前
- `path`: 移動先フォルダの絶対パス（バックスラッシュは `\\` でエスケープ）
- 最大 10 件まで登録可能

## 使い方

1. エクスプローラーでファイルやフォルダを選択
2. 右クリック → 「move to」 → 移動先を選択
3. ファイルが移動されます

※ 同名ファイルがある場合は上書き/スキップ/リネームを選択できます

## アンインストール方法

管理者権限の PowerShell で実行:

```powershell
Set-Location <このフォルダのパス>
powershell -ExecutionPolicy Bypass -File .\uninstall.ps1 -RemoveFiles
```

設定ファイルも削除する場合:

```powershell
powershell -ExecutionPolicy Bypass -File .\uninstall.ps1 -RemoveFiles -RemoveConfig
```

## ライセンス

MIT License
