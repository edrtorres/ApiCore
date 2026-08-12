import express from 'express';
import bodyParser from 'body-parser';
import { createAuthController } from './controllers/authController.mjs';
import { createSupabaseRepository } from './repositories/supabaseRepository.mjs';
import { createLoggerRepository } from './infra/loggerRepository.mjs';
import { createAuthUseCases } from './usecases/authUseCases.mjs';
import { supabaseAdmin } from '../../functions/_shared/supabaseClient.mjs';

export function createApp() {
  const app = express();
  app.use(bodyParser.json());

  // Repositories
  const repo = createSupabaseRepository(supabaseAdmin);
  const logger = createLoggerRepository(supabaseAdmin);

  // Use cases
  const usecases = createAuthUseCases({ repo, logger });

  // Controllers
  const controller = createAuthController(usecases);

  // Routes
  app.post('/register', controller.register);
  app.post('/login', controller.login);
  app.post('/login-by-phone', controller.loginByPhone);
  app.get('/me', controller.me);
  app.post('/logout', controller.logout);
  app.get('/process-auth', controller.processAuth);

  return app;
}
