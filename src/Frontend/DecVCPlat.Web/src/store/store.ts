// © 2024 DecVCPlat. All rights reserved.

import { configureStore } from '@reduxjs/toolkit';
import { persistStore, persistReducer } from 'redux-persist';
import storage from 'redux-persist/lib/storage';
import { combineReducers } from '@reduxjs/toolkit';

// Slices
import authSlice from './slices/authSlice';
import themeSlice from './slices/themeSlice';
import projectSlice from './slices/projectSlice';
import votingSlice from './slices/votingSlice';
import notificationSlice from './slices/notificationSlice';
import walletSlice from './slices/walletSlice';

// Persist configuration
const persistConfig = {
  key: 'decvcplat-root',
  storage,
  whitelist: ['auth', 'theme'], // Only persist auth and theme
};

const rootReducer = combineReducers({
  auth: authSlice,
  theme: themeSlice,
  projects: projectSlice,
  voting: votingSlice,
  notifications: notificationSlice,
  wallet: walletSlice,
});

const persistedReducer = persistReducer(persistConfig, rootReducer);

export const store = configureStore({
  reducer: persistedReducer,
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      serializableCheck: {
        ignoredActions: [
          'persist/PERSIST',
          'persist/REHYDRATE',
          'persist/PAUSE',
          'persist/PURGE',
          'persist/REGISTER',
        ],
      },
    }),
  devTools: process.env.NODE_ENV !== 'production',
});

export const persistor = persistStore(store);

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
