﻿using System;
using System.Collections.Generic;

namespace Projector.Data.Projection
{
    public class Projection : DataProviderBase, IDataConsumer
    {
        private readonly IDictionary<string, IField> _projectionFields;
        private readonly IDictionary<string, ISet<string>> _oldFieldNamesToNewFieldNamesMapping;

        private IDisconnectable _subscription;

        private readonly HashSet<IField> _currentUpdatedFields;

        public Projection(IDataProvider sourceDataProvider, Tuple<IDictionary<string, ISet<string>>, IDictionary<string, IField>> projectionFieldsMeta)
        {
            SetRowIds(sourceDataProvider.RowIds);
            _currentUpdatedFields = new HashSet<IField>();
            _oldFieldNamesToNewFieldNamesMapping = projectionFieldsMeta.Item1;
            _projectionFields = projectionFieldsMeta.Item2;
            _subscription = sourceDataProvider.AddConsumer(this);
        }

        public void OnSchema(ISchema schema)
        {
            var projectedSchema = new ProjectionSchema(schema, _projectionFields);
            SetSchema(projectedSchema);
        }

        public void OnSyncPoint()
        {
            FireChanges();
        }

        public void OnAdd(IReadOnlyCollection<int> ids)
        {
            foreach (var id in ids)
            {
                AddId(id);
            }
        }

        public void OnUpdate(IReadOnlyCollection<int> ids, IReadOnlyCollection<IField> updatedFields)
        {
            _currentUpdatedFields.Clear();
            foreach (var updatedField in updatedFields)
            {
                if (_oldFieldNamesToNewFieldNamesMapping.TryGetValue(updatedField.Name, out ISet<string> newFieldNames))
                {
                    foreach (var newFieldName in newFieldNames)
                    {
                        _currentUpdatedFields.Add(Schema.GetFieldMeta(newFieldName));
                    }
                }
            }

            if (_currentUpdatedFields.Count > 0)
            {
                foreach (var id in ids)
                {
                    UpdateId(id);
                }

                foreach (var updatedField in _currentUpdatedFields)
                {
                    AddUpdatedField(updatedField);
                }
            }
        }

        public void OnDelete(IReadOnlyCollection<int> ids)
        {
            foreach (var id in ids)
            {
                RemoveId(id);
            }
        }
    }
}
