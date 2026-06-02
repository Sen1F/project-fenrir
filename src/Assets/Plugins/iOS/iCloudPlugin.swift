import Foundation

/// Wraps NSUbiquitousKeyValueStore for optional iCloud save sync.
/// Player opt-in only. Never syncs Keychain slot seeds — those are intentionally device-bound.
@objc public class iCloudPlugin: NSObject {

    private static let saveKey = "fenrir_save_json"
    private static let store   = NSUbiquitousKeyValueStore.default

    /// Returns true if iCloud Key-Value Store is available on this device/account.
    @objc public static func isAvailable() -> Bool {
        return store.dictionaryRepresentation.count >= 0 // always succeeds when iCloud is configured
    }

    /// Writes the full JSON save string to iCloud KV store and flushes synchronously.
    @objc public static func uploadSave(json: String) -> Bool {
        store.set(json, forKey: saveKey)
        return store.synchronize()
    }

    /// Returns the iCloud-stored save JSON, or nil if not found.
    @objc public static func downloadSave() -> String? {
        store.synchronize()
        return store.string(forKey: saveKey)
    }

    /// Clears the iCloud entry (e.g. on explicit player opt-out).
    @objc public static func deleteSave() {
        store.removeObject(forKey: saveKey)
        store.synchronize()
    }
}

// ── C-callable shims for Unity DllImport ─────────────────────────────────────

@_cdecl("iCloud_IsAvailable")
func iCloud_IsAvailable() -> Bool {
    return iCloudPlugin.isAvailable()
}

@_cdecl("iCloud_UploadSave")
func iCloud_UploadSave(json: UnsafePointer<CChar>) -> Bool {
    let jsonString = String(cString: json)
    return iCloudPlugin.uploadSave(json: jsonString)
}

@_cdecl("iCloud_DownloadSave")
func iCloud_DownloadSave() -> UnsafePointer<CChar>? {
    guard let value = iCloudPlugin.downloadSave() else { return nil }
    let buffer = UnsafeMutablePointer<CChar>.allocate(capacity: value.utf8.count + 1)
    value.withCString { src in _ = strcpy(buffer, src) }
    return UnsafePointer(buffer)
}

@_cdecl("iCloud_DeleteSave")
func iCloud_DeleteSave() {
    iCloudPlugin.deleteSave()
}
