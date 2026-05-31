export const environment = {
  production: true,
  apiBaseUrl: '/api',  // Served behind nginx reverse proxy in production
  auth: {
    authority: 'https://login.microsoftonline.com/b893e8e7-3737-4ded-baef-d44095250bdc/v2.0',
    clientId: 'b614fb51-d862-44f1-86ac-dc9a54de55e4',
    redirectUrl: 'http://localhost:4200',
    postLogoutRedirectUri: 'http://localhost:4200',
    scope: 'openid profile api://b3bb18e3-c1a1-4519-b90a-2944340a0fb6/access_as_user',
    responseType: 'code',
  }
};
