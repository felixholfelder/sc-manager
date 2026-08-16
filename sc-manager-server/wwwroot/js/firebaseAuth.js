// wwwroot/js/firebaseAuth.js
// Wird per IJSRuntime als ES-Modul importiert - kein <script>-Tag in index.html nötig.

import { initializeApp } from "https://www.gstatic.com/firebasejs/10.12.2/firebase-app.js";
import {
    getAuth,
    signInWithEmailAndPassword,
    createUserWithEmailAndPassword,
    signOut,
    onAuthStateChanged,
    setPersistence,
    browserLocalPersistence
} from "https://www.gstatic.com/firebasejs/10.12.2/firebase-auth.js";

let auth = null;
let dotnetHelper = null;

export function initializeFirebase(config, helper) {
    const app = initializeApp(config);
    auth = getAuth(app);
    dotnetHelper = helper;

    // Anmeldung bleibt auch nach Browser-Neustart erhalten (localStorage)
    setPersistence(auth, browserLocalPersistence).then(() => {
        onAuthStateChanged(auth, (user) => {
            if (!dotnetHelper) return;
            if (user) {
                dotnetHelper.invokeMethodAsync('OnAuthStateChanged', user.email, user.uid);
            } else {
                dotnetHelper.invokeMethodAsync('OnAuthStateChanged', null, null);
            }
        });
    });
}

export async function signIn(email, password) {
    try {
        const cred = await signInWithEmailAndPassword(auth, email, password);
        return { success: true, email: cred.user.email, uid: cred.user.uid, error: null };
    } catch (e) {
        return { success: false, email: null, uid: null, error: mapError(e.code) };
    }
}

export async function register(email, password) {
    try {
        const cred = await createUserWithEmailAndPassword(auth, email, password);
        return { success: true, email: cred.user.email, uid: cred.user.uid, error: null };
    } catch (e) {
        return { success: false, email: null, uid: null, error: mapError(e.code) };
    }
}

export async function logOut() {
    await signOut(auth);
}

function mapError(code) {
    switch (code) {
        case 'auth/invalid-email': return 'Ungültige E-Mail-Adresse.';
        case 'auth/user-disabled': return 'Dieser Benutzer wurde deaktiviert.';
        case 'auth/user-not-found': return 'Kein Benutzer mit dieser E-Mail gefunden.';
        case 'auth/wrong-password': return 'Falsches Passwort.';
        case 'auth/invalid-credential': return 'E-Mail oder Passwort ist falsch.';
        case 'auth/email-already-in-use': return 'Diese E-Mail wird bereits verwendet.';
        case 'auth/weak-password': return 'Das Passwort ist zu schwach (mind. 6 Zeichen).';
        case 'auth/too-many-requests': return 'Zu viele Versuche. Bitte später erneut versuchen.';
        default: return 'Es ist ein Fehler aufgetreten (' + code + ').';
    }
}
