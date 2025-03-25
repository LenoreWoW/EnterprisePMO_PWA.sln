import { createClient } from 'https://cdn.jsdelivr.net/npm/@supabase/supabase-js/+esm';

const supabaseUrl = 'https://db.pffjdvahsgmtybxrhnla.supabase.co';
const supabaseAnonKey = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InBmZmpkdmFoc2dtdHlieHJobmxhIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDI2ODk3NDYsImV4cCI6MjA1ODI2NTc0Nn0.6OPXiW2QwxvA42F1XcN83bHmtdM7NhulvDaqXxIE9hk';
export const supabase = createClient(supabaseUrl, supabaseAnonKey);

export async function signIn(email, password) {
    const { data, error } = await supabase.auth.signIn({ email, password });
    if (error) throw error;
    return data;
}

export async function signUp(email, password) {
    const { data, error } = await supabase.auth.signUp({ email, password });
    if (error) throw error;
    return data;
}

export function signOut() {
    return supabase.auth.signOut();
}
