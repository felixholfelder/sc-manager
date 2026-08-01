// wwwroot/js/firestore.js
// Wird per IJSRuntime als ES-Modul importiert.

import { initializeApp, getApps, getApp } from "https://www.gstatic.com/firebasejs/10.12.2/firebase-app.js";
import {
    getFirestore,
    collection,
    doc,
    addDoc,
    getDoc,
    getDocs,
    updateDoc,
    deleteDoc,
    query,
    orderBy,
    serverTimestamp,
    where
} from "https://www.gstatic.com/firebasejs/10.12.2/firebase-firestore.js";

let db = null;

export function initializeFirestore(config) {
    // Falls die Firebase-App bereits (z. B. durch firebaseAuth.js) initialisiert wurde,
    // diese wiederverwenden statt einen Fehler "app already exists" zu provozieren.
    const app = getApps().length ? getApp() : initializeApp(config);
    db = getFirestore(app);
}

export async function addItem(collectionName, data) {
    const ref = await addDoc(collection(db, collectionName), {
        ...data,
        createdAt: serverTimestamp()
    });
    return ref.id;
}

export async function getItem(collectionName, id) {
    const snap = await getDoc(doc(db, collectionName, id));
    return snap.exists() ? serializeDoc(snap.id, snap.data()) : null;
}

export async function getItems(collectionName) {
    const q = query(collection(db, collectionName), orderBy('createdAt', 'desc'));
    const snap = await getDocs(q);
    return snap.docs.map(d => serializeDoc(d.id, d.data()));
}

export async function updateItem(collectionName, id, data) {
    await updateDoc(doc(db, collectionName, id), data);
}

export async function deleteItem(collectionName, id) {
    await deleteDoc(doc(db, collectionName, id));
}

export async function getItemsByField(collectionName, field, value) {
    const q = query(collection(db, collectionName), where(field, '==', value));
    const snap = await getDocs(q);
    return snap.docs.map(d => serializeDoc(d.id, d.data()));
}

export async function getItemByField(collectionName, field, value) {
    const q = query(collection(db, collectionName), where(field, '==', value));
    const snap = await getDocs(q);
    if (snap.docs.length > 1) {
        throw new Exception("More than one document found: " + value)
    }
    return serializeDoc(snap.docs[0].id, snap.docs[0].data());
}

// Wandelt Firestore-Timestamps in ISO-Strings um, damit JSInterop
// die Daten sauber nach C# serialisieren kann.
function serializeDoc(id, data) {
    const result = { documentId: id };
    for (const [key, value] of Object.entries(data)) {
        result[key] = (value && typeof value.toDate === 'function')
            ? value.toDate().toISOString()
            : value;
    }
    return result;
}
