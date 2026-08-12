export function createAuthController(usecases) {
  return {
    register: async (req, res) => {
      try {
        const result = await usecases.register(req.body);
        return res.status(result.status).json({ message: result.message });
      } catch (err) {
        return res.status(500).json({ message: 'No se pudo crear la cuenta, intenta nuevamente' });
      }
    },

    login: async (req, res) => {
      try {
        const result = await usecases.login(req.body);
        return res.status(result.status).json(result.body);
      } catch (err) {
        return res.status(500).json({ message: 'No se pudo iniciar sesión' });
      }
    },

    loginByPhone: async (req, res) => {
      try {
        const result = await usecases.loginByPhone(req.body);
        return res.status(result.status).json({ message: result.message });
      } catch (err) {
        return res.status(500).json({ message: 'No se pudo procesar la solicitud' });
      }
    },

    me: async (req, res) => {
      try {
        const auth = req.headers.authorization || '';
        const result = await usecases.me(auth);
        return res.status(result.status).json(result.body);
      } catch (err) {
        return res.status(500).json({ message: 'No se pudo verificar la sesión' });
      }
    },

    logout: async (req, res) => {
      try {
        const result = await usecases.logout(req.body);
        return res.status(result.status).json({ message: result.message });
      } catch (err) {
        return res.status(500).json({ message: 'No se pudo cerrar la sesión' });
      }
    },

    processAuth: async (req, res) => {
      try {
        const result = await usecases.processAuth(req.query);
        if (result.redirect) return res.redirect(result.redirect);
        return res.status(result.status).json({ message: result.message });
      } catch (err) {
        return res.status(500).json({ message: 'No se pudo procesar la autenticación' });
      }
    }
  };
}
