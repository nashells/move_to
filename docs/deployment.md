# デプロイ / シェル登録ガイド

## 目的
コンテキストメニュー型シェル拡張の登録・配布・解除手順をまとめる。

## 対象
- OS: Windows 11（新コンテキストメニュー前提）
- 配布物: シェル拡張 DLL、登録/解除スクリプト（regasm など）、設定ファイルの配置案内

## 登録方式（確定値）
- COM 登録: regasm (.NET Framework 4.x ツール) で net48 ビルドの DLL を登録
- ProgID: `Nashells.MoveTo.ContextMenu`
- GUID: `{D8E8C7DA-5C4E-4B61-9A1F-4C8E9C9B7F2B}`
- 配置パス（固定）: `C:\Program Files\nashells\MoveTo\`
- /codebase を利用し、コードとスクリプト双方で上記パスを一致させる
- 登録・解除は管理者権限で実行する

## 必要な手順
1. シェル拡張 DLL の配置（例: `MoveTo.Shell.dll`, `MoveTo.Core.dll`, SharpShell 依存を `C:\Program Files\nashells\MoveTo\` にコピー）
2. regasm による登録（例）
   ```powershell
   $dll = "C:\\Program Files\\nashells\\MoveTo\\MoveTo.Shell.dll"
   & "C:\\Windows\\Microsoft.NET\\Framework64\\v4.0.30319\\regasm.exe" /codebase $dll
   ```
3. Explorer 再起動（必要に応じて）
   ```powershell
   Stop-Process -Name explorer -Force
   Start-Process explorer
   ```
4. 解除手順
   ```powershell
   $dll = "C:\\Program Files\\nashells\\MoveTo\\MoveTo.Shell.dll"
   & "C:\\Windows\\Microsoft.NET\\Framework64\\v4.0.30319\\regasm.exe" /unregister $dll
   ```

- Approved キー登録（必要な場合）
   ```powershell
   reg add "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved" /v "{D8E8C7DA-5C4E-4B61-9A1F-4C8E9C9B7F2B}" /t REG_SZ /d "MoveTo context menu" /f
   ```

## レジストリ確認の目安
- ProgID キー: `HKCR\Nashells.MoveTo.ContextMenu` に既定値 `MoveTo.Shell.MoveToContextMenu`、配下に `CLSID` サブキー
- CLSID キー: `HKCR\CLSID\{D8E8C7DA-5C4E-4B61-9A1F-4C8E9C9B7F2B}\InprocServer32`
   - 既定値: `mscoree.dll`
   - `Class`: `MoveTo.Shell.MoveToContextMenu`
   - `Assembly`: `MoveTo.Shell, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null`
   - `RuntimeVersion`: `v4.0.30319`
   - `CodeBase`: `file:///C:/Program Files/nashells/MoveTo/MoveTo.Shell.dll`

## スクリプト（管理者 PowerShell）
- インストール: `scripts/install.ps1` （コピー → regasm 登録 → config 初期化、`-RestartExplorer` で再起動）
- アンインストール: `scripts/uninstall.ps1` （regasm 解除、Approved エントリ削除、`-RemoveFiles` で配置削除、`-RestartExplorer` で再起動）

## インストーラ/スクリプトの要件
- 管理者権限で実行
- 登録・解除の両方を提供
- 失敗時のログを残す
- Explorer 再起動を案内（自動/手動いずれか）
- `%LOCALAPPDATA%\MoveTo\config.json` を未作成時に初期生成またはサンプルコピー

## 決定済み / 残課題
- 決定済み: ProgID/GUID、固定配置パス、regasm /codebase 方式
- 残課題: regasm 以外の配布手段（MSIX など）の要否、コードサイン有無、バージョンアップ手順の詳細
