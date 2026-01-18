# Error Handling and Cancellation Flow in FtpManager

## Overview
This document explains how errors and cancellations are handled in the FTP download operations.

## Cancellation Token Flow

### 1. **User Clicks Cancel**
```
DownloadProgressWindow.CancelButton_Click()
  ??> _cancellationTokenSource.Cancel()
      ??> CancellationToken is signaled
```

### 2. **Cancellation Propagation**
The cancellation flows through these methods:

```
DownloadRepositoryAsync/DownloadFolderAsync (Top Level)
  ??> BuildDirectoryCache
  ?     ??> ScanDirectoryRecursive (checks token, throws OperationCanceledException)
  ?           ??> Parallel.ForEach (respects CancellationToken in ParallelOptions)
  ?
  ??> DownloadDirectoryFromCache
        ??> Task.WhenAll(downloadTasks)
              ??> Individual download tasks
                    ??> DownloadFileWithProgress
                          ??> Checks token during file read loop
```

### 3. **Cancellation Check Points**
- **Before each major operation**: `cancellationToken.ThrowIfCancellationRequested()`
- **In SemaphoreSlim waits**: `semaphore.WaitAsync(cancellationToken)`
- **During file downloads**: Checked every buffer read (64KB chunks)
- **In parallel operations**: `ParallelOptions { CancellationToken = cancellationToken }`

## Exception Handling Strategy

### **Three-Layer Exception Handling**

#### **Layer 1: Top-Level Methods** (User-Facing)
- **Methods**: `DownloadRepositoryAsync`, `DownloadFolderAsync`
- **Behavior**:
  - Catches `OperationCanceledException` ? Reports "Download cancelled by user." ? Returns `false`
  - Catches other exceptions ? Reports error ? Shows MessageBox ? Returns `false`
- **Responsibility**: Final user notification and graceful failure

```csharp
catch (OperationCanceledException)
{
    progress?.Report("Download cancelled by user.");
    return false;
}
catch (Exception ex)
{
    progress?.Report($"Error: {ex.Message}");
    MessageBox.Show($"Failed to download: {ex.Message}", "Download Error", ...);
    return false;
}
```

#### **Layer 2: Operation Methods** (Download/Scan Logic)
- **Methods**: `DownloadDirectoryFromCache`, `ScanDirectoryRecursive`, `DownloadDirectory`
- **Behavior**:
  - Catches `OperationCanceledException` ? Re-throws WITHOUT reporting (avoid duplicate messages)
  - Catches other exceptions ? Reports specific error ? Re-throws (let top level handle)
- **Responsibility**: Specific error context, cleanup, propagation

```csharp
catch (OperationCanceledException)
{
    // Clean up resources (e.g., semaphore.Release())
    throw; // Let top level handle
}
catch (Exception ex)
{
    progress?.Report($"Error downloading {currentPath}: {ex.Message}");
    // Clean up resources
    throw; // Let top level decide what to do
}
```

#### **Layer 3: Individual Tasks** (File Operations)
- **Methods**: `DownloadFileWithProgress`
- **Behavior**:
  - Catches `OperationCanceledException` ? Deletes partial file ? Re-throws
  - Catches other exceptions ? Reports failure ? Wraps and re-throws
- **Responsibility**: File-level cleanup, detailed error info

```csharp
catch (OperationCanceledException)
{
    // Clean up partial file
    if (File.Exists(destinationPath))
        File.Delete(destinationPath);
    throw;
}
catch (Exception ex)
{
    progress?.Report($"Failed to download {Path.GetFileName(destinationPath)}: {ex.Message}");
    throw new Exception($"Failed to download file from {ftpUri}: {ex.Message}", ex);
}
```

## Progress Reporting During Cancellation

### **What the User Sees**
When user cancels during download:

```
Building folder structure cache...
Scanned: / (10 items)
Scanned: /mods (50 items)
Found 1000 files to check...
200 files need to be downloaded, 800 files up-to-date
Downloading: ace_medical.pbo => C:\mods\@ACE\addons\ace_medical.pbo (2.5 MB)
Downloading: ace_common.pbo => C:\mods\@ACE\addons\ace_common.pbo (1.8 MB)
[User clicks Cancel]
Download cancelled.              <-- From inner catch (Task.WhenAll)
Download cancelled by user.      <-- From top level catch
```

### **Why Two "Cancelled" Messages?**
1. **"Download cancelled."** - From `DownloadDirectoryFromCache` catching `Task.WhenAll` exception
2. **"Download cancelled by user."** - From `DownloadRepositoryAsync` catching the re-thrown exception

This is intentional to show the cancellation propagated correctly.

## Key Behaviors

### ? **Cancellation**
- Stops immediately when token is signaled
- Cleans up partial files
- Releases semaphores
- Propagates to calling code
- User sees clear "cancelled" message

### ? **Network Errors**
- Reports specific error (connection, timeout, authentication)
- Cleans up resources
- Shows MessageBox to user
- Returns `false` to indicate failure
- Does NOT crash the application

### ? **File System Errors**
- Reports which file failed
- Continues with other files (doesn't stop entire download)
- Collects error count
- User can see which files failed in progress log

### ? **Parallel Downloads**
- Up to 8 concurrent downloads
- Individual failures don't stop others
- Semaphore ensures resource limits
- Cancellation stops all tasks

## Testing Cancellation

### **Verify Proper Cancellation**
1. Start downloading a large repository
2. Wait for files to start downloading (see progress messages)
3. Click "Cancel" button
4. **Expected Results**:
   - ? Progress stops updating immediately
   - ? No new "Downloading:" messages appear
   - ? See "Download cancelled." message
   - ? See "Download cancelled by user." message
   - ? Button changes to "Close"
   - ? Partial files are deleted
   - ? FTP server stops receiving requests (check server logs)

### **Verify Proper Error Handling**
1. Disconnect network during download
2. **Expected Results**:
   - ? See specific error messages for failed files
   - ? MessageBox appears with error
   - ? Application doesn't crash
   - ? Can try again

## Summary

The error handling and cancellation system is designed with these principles:

1. **Immediate Response**: Cancellation checks at every major operation
2. **Clean Propagation**: Exceptions bubble up through layers
3. **User Clarity**: Clear, non-technical messages in UI
4. **Resource Safety**: Always cleanup (files, semaphores, connections)
5. **Graceful Degradation**: Errors don't crash the app
6. **Debug-Friendly**: Detailed error context in progress log
7. **No Silent Failures**: Every error is reported and logged
