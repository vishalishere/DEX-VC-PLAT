// © 2024 DecVCPlat. All rights reserved.

import { createSlice, PayloadAction } from '@reduxjs/toolkit';

interface ThemeState {
  isDarkMode: boolean;
  primaryColor: string;
  accentColor: string;
}

const initialState: ThemeState = {
  isDarkMode: false,
  primaryColor: '#1976d2',
  accentColor: '#f50057',
};

const themeSlice = createSlice({
  name: 'theme',
  initialState,
  reducers: {
    toggleDarkMode: (state) => {
      state.isDarkMode = !state.isDarkMode;
    },
    setDarkMode: (state, action: PayloadAction<boolean>) => {
      state.isDarkMode = action.payload;
    },
    setPrimaryColor: (state, action: PayloadAction<string>) => {
      state.primaryColor = action.payload;
    },
    setAccentColor: (state, action: PayloadAction<string>) => {
      state.accentColor = action.payload;
    },
    resetTheme: (state) => {
      state.isDarkMode = false;
      state.primaryColor = '#1976d2';
      state.accentColor = '#f50057';
    },
  },
});

export const { 
  toggleDarkMode, 
  setDarkMode, 
  setPrimaryColor, 
  setAccentColor, 
  resetTheme 
} = themeSlice.actions;

export default themeSlice.reducer;
