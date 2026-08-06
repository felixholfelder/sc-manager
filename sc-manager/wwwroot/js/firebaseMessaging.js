import { initializeApp, getApps, getApp } from "https://www.gstatic.com/firebasejs/10.12.2/firebase-app.js";
import { getMessaging, getToken, onMessage } from "https://www.gstatic.com/firebasejs/10.12.2/firebase-messaging.js";

let messaging;

export function initializeMessaging(config) {
    const app = getApps().length ? getApp() : initializeApp(config);
    messaging = getMessaging(app);
}

export async function requestPermissionAndGetToken(vapidKey, firebaseConfig) {
    try {
        const permission = await Notification.requestPermission();
        if (permission !== "granted") return null;

        // Config als Query-String an die SW-URL anhängen
        const params = new URLSearchParams({
            apiKey: firebaseConfig.apiKey,
            authDomain: firebaseConfig.authDomain,
            projectId: firebaseConfig.projectId,
            storageBucket: firebaseConfig.storageBucket,
            messagingSenderId: firebaseConfig.messagingSenderId,
            appId: firebaseConfig.appId
        });

        const registration = await navigator.serviceWorker.register(
            `/firebaseMessagingSW.js?${params.toString()}`
        );

        const token = await getToken(messaging, {
            vapidKey: vapidKey,
            serviceWorkerRegistration: registration
        });
        return token;
    } catch (err) {
        console.error("FCM Token Error:", err);
        return null;
    }
}

export function registerOnMessage(dotNetHelper) {
    onMessage(messaging, (payload) => {
        dotNetHelper.invokeMethodAsync(
            "HandleIncomingMessage",
            payload.notification?.title,
            payload.notification?.body
        );
    });
}