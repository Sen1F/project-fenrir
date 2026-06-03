import Foundation
import Security

@objc public class KeychainPlugin: NSObject {

    private static let service = "com.fenrir.projectfenrir"

    @objc public static func getSeed(key: String) -> String? {
        let query: [String: Any] = [
            kSecClass as String:            kSecClassGenericPassword,
            kSecAttrService as String:      service,
            kSecAttrAccount as String:      key,
            kSecReturnData as String:       true,
            kSecMatchLimit as String:       kSecMatchLimitOne
        ]

        var result: AnyObject?
        let status = SecItemCopyMatching(query as CFDictionary, &result)

        guard status == errSecSuccess,
              let data = result as? Data,
              let value = String(data: data, encoding: .utf8)
        else { return nil }

        return value
    }

    @objc public static func setSeed(key: String, value: String) -> Bool {
        guard let data = value.data(using: .utf8) else { return false }

        // Delete any existing entry first
        let deleteQuery: [String: Any] = [
            kSecClass as String:        kSecClassGenericPassword,
            kSecAttrService as String:  service,
            kSecAttrAccount as String:  key
        ]
        SecItemDelete(deleteQuery as CFDictionary)

        let addQuery: [String: Any] = [
            kSecClass as String:                kSecClassGenericPassword,
            kSecAttrService as String:          service,
            kSecAttrAccount as String:          key,
            kSecValueData as String:            data,
            kSecAttrAccessible as String:       kSecAttrAccessibleAfterFirstUnlockThisDeviceOnly
        ]

        let status = SecItemAdd(addQuery as CFDictionary, nil)
        return status == errSecSuccess
    }
}

// ── C-callable shims for Unity DllImport ─────────────────────────────────────

@_cdecl("Keychain_GetSeed")
func Keychain_GetSeed(key: UnsafePointer<CChar>) -> UnsafePointer<CChar>? {
    let keyString = String(cString: key)
    guard let value = KeychainPlugin.getSeed(key: keyString) else { return nil }
    // Caller does not free — returned pointer lives in a static buffer per call.
    // Unity marshals the value before the next call.
    let buffer = UnsafeMutablePointer<CChar>.allocate(capacity: value.utf8.count + 1)
    value.withCString { src in _ = strcpy(buffer, src) }
    return UnsafePointer(buffer)
}

@_cdecl("Keychain_SetSeed")
func Keychain_SetSeed(key: UnsafePointer<CChar>, value: UnsafePointer<CChar>) -> Bool {
    let keyString   = String(cString: key)
    let valueString = String(cString: value)
    return KeychainPlugin.setSeed(key: keyString, value: valueString)
}
