import { createApp } from './src/server.mjs';

const port = process.env.PORT || 3000;
const app = createApp();
app.listen(port, () => console.log(`Auth service listening on ${port}`));
