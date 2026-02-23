/**
 * Copy all data from KSS_Common_Prod to KSS_Common_Dev.
 *
 * Usage: node copy-prod-to-dev.js
 * Requires: npm install mssql
 */

const sql = require('mssql');

const baseConfig = {
  user: 'kss',
  password: 'HZcg4!fjq0PQoO@%JFDn',
  server: 'database91.sebaoffice.ir',
  options: {
    encrypt: false,
    trustServerCertificate: true,
  },
};

const prodConfig = { ...baseConfig, database: 'KSS_Common_Prod' };
const devConfig = { ...baseConfig, database: 'KSS_Common_Dev' };

// Tables in dependency order (parents first for insert)
const TABLES_INSERT_ORDER = [
  'Language',
  'Country', 'CountryTranslation',
  'Region', 'RegionTranslation',
  'City', 'CityTranslation',
];

const TABLES_DELETE_ORDER = [...TABLES_INSERT_ORDER].reverse();

// Tables with IDENTITY columns
const IDENTITY_TABLES = ['Language', 'Country', 'Region', 'City'];

async function main() {
  const prodPool = await sql.connect(prodConfig);
  console.log('Connected to PROD');

  // Count prod data
  console.log('\n=== PROD DATA COUNTS ===');
  for (const table of TABLES_INSERT_ORDER) {
    const res = await prodPool.request().query(`SELECT COUNT(*) AS cnt FROM dbo.[${table}]`);
    console.log(`  ${table}: ${res.recordset[0].cnt}`);
  }

  await prodPool.close();

  // Now connect to DEV and copy
  const devPool = await sql.connect(devConfig);
  console.log('\nConnected to DEV');

  const transaction = new sql.Transaction(devPool);
  await transaction.begin();
  console.log('Transaction started');

  try {
    // Delete all data in DEV (reverse dependency order)
    console.log('\n=== CLEARING DEV DATABASE ===');
    for (const table of TABLES_DELETE_ORDER) {
      const res = await transaction.request().query(`DELETE FROM dbo.[${table}]`);
      console.log(`  Deleted from ${table}: ${res.rowsAffected[0]} rows`);
    }

    // Copy data from PROD to DEV using cross-database SELECT
    console.log('\n=== COPYING PROD -> DEV ===');
    for (const table of TABLES_INSERT_ORDER) {
      const hasIdentity = IDENTITY_TABLES.includes(table);

      let query = '';
      if (hasIdentity) {
        const colResult = await transaction.request().query(`
          SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
          WHERE TABLE_NAME = '${table}' AND TABLE_SCHEMA = 'dbo'
          ORDER BY ORDINAL_POSITION
        `);
        const cols = colResult.recordset.map(r => `[${r.COLUMN_NAME}]`).join(', ');
        query = `SET IDENTITY_INSERT dbo.[${table}] ON; INSERT INTO dbo.[${table}] (${cols}) SELECT ${cols} FROM [KSS_Common_Prod].dbo.[${table}]; SET IDENTITY_INSERT dbo.[${table}] OFF;`;
      } else {
        query = `INSERT INTO dbo.[${table}] SELECT * FROM [KSS_Common_Prod].dbo.[${table}]`;
      }

      const res = await transaction.request().query(query);
      console.log(`  Copied ${table}: ${res.rowsAffected[0]} rows`);
    }

    await transaction.commit();
    console.log('\n=== SUCCESS: All data copied from PROD to DEV ===');

    // Verify DEV counts
    console.log('\n=== DEV DATA COUNTS (verification) ===');
    for (const table of TABLES_INSERT_ORDER) {
      const res = await devPool.request().query(`SELECT COUNT(*) AS cnt FROM dbo.[${table}]`);
      console.log(`  ${table}: ${res.recordset[0].cnt}`);
    }

  } catch (err) {
    await transaction.rollback();
    console.error('\nERROR - Transaction rolled back:', err.message);
    throw err;
  } finally {
    await devPool.close();
  }
}

main().catch(err => {
  console.error('Fatal error:', err);
  process.exit(1);
});
