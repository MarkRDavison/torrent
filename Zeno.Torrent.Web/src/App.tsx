import React from 'react';
import { BrowserRouter } from 'react-router-dom';
import AppWithRouterAccess from './AppWithRouterAccess';
import { AuthProvider } from './components/AuthContext';

function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
          <AppWithRouterAccess />
      </BrowserRouter>
    </AuthProvider>
  );
}

export default App;
