# デプロイ / シェル登録ガイド

## 目的
コンテキストメニュー型シェル拡張の登録・配布・解除手順をまとめる。

## 対象
- OS: Windows 11（新コンテキストメニュー前提）
- 配布物: シェル拡張 DLL、登録/解除スクリプト（regasm など）、設定ファイルの配置案内

## 登録方式（想定）
- COM 登録: regasm (.NET 8/10 対応) を使用
- ProgID/GUID: 固定値を決定し、コードとスクリプト双方で一致させる
- /codebase オプションを利用する場合は配置パスを固定する
- 登録・解除は管理者権限で実行する

## 必要な手順
1. シェル拡張 DLL の配置
2. regasm による登録（例）
   ```powershell
   $dll = "C:\\Path\\To\\MoveTo.Shell.dll"
   & "C:\\Windows\\Microsoft.NET\\Framework64\\v4.0.30319\\regasm.exe" /codebase $dll
   ```
3. Explorer 再起動（必要に応じて）
   ```powershell
   Stop-Process -Name explorer -Force
   Start-Process explorer
   ```
4. 解除手順
   ```powershell
   & "C:\\Windows\\Microsoft.NET\\Framework64\\v4.0.30319\\regasm.exe" /unregister $dll
   ```

## インストーラ/スクリプトの要件
- 管理者権限で実行
- 登録・解除の両方を提供
- 失敗時のログを残す
- Explorer 再起動を案内

## 今後決定すべき事項
- ProgID/GUID の具体値
- 配置先パスの固定方針
- regasm 以外の配布手段（MSIX/Installer）の要否
- 設定ファイルの初期配置と更新ポリシー
